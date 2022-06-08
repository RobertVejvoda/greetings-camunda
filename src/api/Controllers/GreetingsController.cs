using Dapr;
using Dapr.Client;
using greetings_camunda.Command;
using greetings_camunda.Extensions;
using greetings_camunda.Model;
using greetings_camunda.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace greetings_camunda.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class GreetingsController : ControllerBase
	{
		private readonly ILogger<GreetingsController> logger;
		private readonly IGreetingService greetingService;

		public GreetingsController(
						IGreetingService greetingService,
						ILogger<GreetingsController> logger)
		{
			this.greetingService = greetingService;
			this.logger = logger;
		}

		[HttpGet("/")]
		public IActionResult Get()
		{
			return Ok("Ready");
		}

		/// <summary>
		/// Receing a message to MQTT pubsub and topic named "camunda/greeting-requested" 
		/// and it should start a new Camunda Greet process by invoking zeebe-command binding.
		/// </summary>
		[Topic("mqtt", "camunda/greeting-requested")]
		[HttpPost("/greeting-requested")]
		public async Task<ActionResult> GreetingRequested([FromBody, Required] GreetingRequest request, [FromServices] DaprClient daprClient)
		{
			logger.LogInformation(request.ToString());

			// serialize and deserialize request to get it into dictionary format.
			var json = JsonConvert.SerializeObject(request);

			// start Camunda process by invoking a message to Camunda
			await daprClient.InvokeBindingAsync<PublishMessageRequest, PublishMessageResponse>("zeebe-command", Commands.PublishMessage,
				new PublishMessageRequest("greeting-requested", request.GreetingId.ToString(), string.Concat("decide-greeting-", request.GreetingId), "10s", // TTL
				JsonConvert.DeserializeObject<Dictionary<string, string>>(json)));

			return Ok();
		}

		/// <summary>
		/// Decide greeting and set variables to Camunda as global.
		/// </summary>
		[HttpPost("/decide-greeting")]
        public async Task<ActionResult> DecideHowToGreet([FromServices] DaprClient daprClient)
        {
			// logger.LogZeebeHeaders(Request);

			// randomly choose time of day
			var timeSpan = TimeSpan.FromSeconds(Random.Shared.Next(0, 24*60*60));
			var greeting = greetingService.DecideGreeting(TimeOnly.FromTimeSpan(timeSpan));
            logger.LogInformation(greeting);

			// get the element instance key where to set the variables back to Camunda
			var elementInstanceKey = long.Parse(Request.Headers["X-Zeebe-Element-Instance-Key"]);
			await daprClient.InvokeBindingAsync<SetVariablesRequest, SetVariablesResponse>("zeebe-command", Commands.SetVariables,
				new SetVariablesRequest(elementInstanceKey, new { Greeting = greeting }, false));

			return Ok();
        }

        /// <summary>
        /// Greet from Zeebe Broker output binding
        /// </summary>
        [HttpPost("/greet")]
		public async Task<ActionResult> Greet([FromBody] GreetingRequest greeting, [FromServices] DaprClient daprClient)
		{
			//logger.LogZeebeHeaders(Request);
			logger.LogInformation(greeting.Greeting);

			// simulate process error, pseudo-randomly throw errors
			if (Random.Shared.NextDouble() > 0.8)
			{
                string message = $"No greeting for {greeting.Name}!";
                logger.LogWarning(message);
				
				var jobKey = long.Parse(Request.Headers["X-Zeebe-Job-Key"]);
				var throwError = new ThrowErrorRequest(jobKey, "Greeting Error", message);
				
				Response.Headers.Add("X-Custom-ErrorCode", "Greeting Error");
				Response.Headers.Add("X-Custom-ErrorMessage", message);

				// dictionary convert hack
				await daprClient.InvokeBindingAsync<ThrowErrorRequest, ThrowErrorResponse>("zeebe-command", Commands.ThrowError, throwError);
			}
			else
            {
				var message = greetingService.Greet(greeting.Greeting, greeting.Name);

				// get the element instance key where to set the variables back to Camunda
				var elementInstanceKey = long.Parse(Request.Headers["X-Zeebe-Element-Instance-Key"]);
				await daprClient.InvokeBindingAsync<SetVariablesRequest, SetVariablesResponse>("zeebe-command", Commands.SetVariables,
					new SetVariablesRequest(elementInstanceKey, new { GreetingMessage = message }, false));
			}

			return Ok();
		}

		/// <summary>
		/// Email from Zeebe Broker output binding
		/// </summary>
		[HttpPost("/send-email")]
		public async Task<ActionResult> SendEmail([FromBody, Required] GreetingRequest greeting, [FromServices] DaprClient daprClient)
		{
            var body = greeting.GreetingMessage;
            var metadata = new Dictionary<string, string>
            {
                ["emailFrom"] = "noreply@incredible.inc",
                ["emailTo"] = greeting.Email,
                ["subject"] = $"Greetings for {greeting.Name}!"
            };

            await daprClient.InvokeBindingAsync("sendmail", "create", body, metadata);

            return Ok();
		}

		/// <summary>
		/// Admin Email from Zeebe Broker output binding
		/// </summary>
		[HttpPost("/send-email-admin")]
		public async Task<ActionResult> SendEmailAdmin([FromServices] DaprClient daprClient)
		{
			logger.LogZeebeHeaders(Request);

			// var jobKey = long.Parse(Request.Headers["X-Zeebe-Job-Key"]);
			var jobKey = Request.Headers["X-Zeebe-Job-Key"];
			var errorCode = Request.Headers["X-Custom-ErrorCode"];
			var errorMessage = Request.Headers["X-Custom-ErrorMessage"];
			var body = $"Job {jobKey} failed. {errorMessage}";
			var metadata = new Dictionary<string, string>
			{
				["emailFrom"] = "noreply@incredible.inc",
				["emailTo"] = "admin@incredible.inc",
				["subject"] = errorCode
			};

			await daprClient.InvokeBindingAsync("sendmail", "create", body, metadata);

			return Ok();
		}
	}
}
