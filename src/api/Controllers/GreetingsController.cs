using Dapr;
using Dapr.Client;
using greetings_camunda.Command;
using greetings_camunda.Extensions;
using greetings_camunda.Model;
using greetings_camunda.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
		private readonly JsonSerializerSettings jsonSerializerSettings;

        public GreetingsController(ILogger<GreetingsController> logger)
		{
            this.logger = logger;
			this.jsonSerializerSettings = new JsonSerializerSettings 
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
		}

		[HttpGet("/")]
		public IActionResult Get()
		{
			return Ok("Ready");
		}

		/// <summary>
		/// Receiving a message to MQTT pubsub and topic named "camunda/greeting-requested" 
		/// and it should start a new Camunda Greet process by invoking zeebe-command binding.
		/// </summary>
		[Topic("mqtt", "camunda/greeting-requested")]
		[HttpPost("/greeting-requested")]
		public async Task<ActionResult> GreetingRequested([FromBody, Required] GreetingRequest request, [FromServices] DaprClient daprClient)
		{
			logger.LogInformation(request.ToString());

			// serialize and deserialize request to get it into dictionary format.
			var json = JsonConvert.SerializeObject(request, Formatting.Indented, jsonSerializerSettings);

			// start Camunda process by invoking a message to Camunda
			await daprClient.InvokeBindingAsync<PublishMessageRequest, PublishMessageResponse>("zeebe-command", Commands.PublishMessage,
				new PublishMessageRequest("greeting-requested", request.GreetingId.ToString(), string.Concat("decide-greeting-", request.GreetingId), "10s", // TTL
				JsonConvert.DeserializeObject<Dictionary<string, string>>(json)));

			return Ok();
		}

		/// <summary>
        /// Score from Zeebe Broker output binding
        /// </summary>
		[HttpPost("/score")]
		public async Task<ActionResult> Score([FromBody, Required] GreetingRequest request,
			[FromServices] DaprClient daprClient, [FromServices] IScoringService scoringService)
		{
			request.Score = await scoringService.Score(request.Email);
			
			var elementInstanceKey = long.Parse(Request.Headers["X-Zeebe-Element-Instance-Key"]);
			await daprClient.InvokeBindingAsync<SetVariablesRequest, SetVariablesResponse>("zeebe-command", Commands.SetVariables,
				new SetVariablesRequest(elementInstanceKey, request, false));

			return Ok();
		}

        /// <summary>
        /// Greet from Zeebe Broker output binding
        /// </summary>
        [HttpPost("/greet")]
		public async Task<ActionResult> Greet([FromBody] GreetingRequest greeting, [FromServices] DaprClient daprClient)
		{
			if (greeting.Score < 0.3)
			{
				var message = $"{greeting.Name} doesn't deserve greeting due to low score.";
				
				logger.LogWarning(message);
				
				var jobKey = long.Parse(Request.Headers["X-Zeebe-Job-Key"]);
				var throwErrorRequest = new ThrowErrorRequest(jobKey, "GreetingError", message);
		 		var elementInstanceKey = long.Parse(Request.Headers["X-Zeebe-Element-Instance-Key"]);
				await daprClient.InvokeBindingAsync<SetVariablesRequest, SetVariablesResponse>("zeebe-command", Commands.SetVariables,
					new SetVariablesRequest(elementInstanceKey, throwErrorRequest, false));	

				await daprClient.InvokeBindingAsync<ThrowErrorRequest, ThrowErrorResponse>("zeebe-command", Commands.ThrowError, throwErrorRequest);
			}

			return Ok();
		}

		/// <summary>
		/// Email from Zeebe Broker output binding
		/// </summary>
		[HttpPost("/send-email")]
		public async Task<ActionResult> SendEmail([FromBody, Required] GreetingRequest greeting, [FromServices] DaprClient daprClient)
		{
            var body = ComposeGreetingMessage(greeting);
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
		public async Task<ActionResult> SendEmailAdmin([FromBody, Required] ThrowErrorRequest greetingError, [FromServices] DaprClient daprClient)
		{
			var jobKey = long.Parse(Request.Headers["X-Zeebe-Job-Key"]);
			var body = greetingError.ErrorMessage;
			var metadata = new Dictionary<string, string>
			{
				["emailFrom"] = "noreply@incredible.inc",
				["emailTo"] = "admin@incredible.inc",
				["subject"] = $"Greeter job {jobKey} failed."
			};

			await daprClient.InvokeBindingAsync("sendmail", "create", body, metadata);

			return Ok();
		}

        private static string ComposeGreetingMessage(GreetingRequest greeting) => $"{greeting.Greeting} {greeting.Name}!";

	}
}
