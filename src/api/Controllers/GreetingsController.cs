using Dapr;
using Dapr.Client;
using greetings_camunda.Command;
using greetings_camunda.Model;
using greetings_camunda.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
		[HttpPost("/greeting-request")]
		public async Task<ActionResult> GreetingRequested([FromBody, Required] GreetingRequest request, [FromServices] DaprClient daprClient)
		{
			logger.LogInformation(request.ToString());

			// serialize and deserialize request to get it into dictionary format.
			var json = JsonConvert.SerializeObject(request, jsonSerializerSettings);

			// start Camunda process by invoking a message to Camunda
			await daprClient.InvokeBindingAsync<PublishMessageRequest, PublishMessageResponse>(Bindings.ZeebeCommand, Commands.PublishMessage,
				new PublishMessageRequest("greeting-requested", request.GreetingId.ToString(), string.Concat("decide-greeting-", request.GreetingId), "10s", // TTL
				JsonConvert.DeserializeObject<Dictionary<string, string>>(json)));

			return Ok();
		}

		/// <summary>
        /// Score person's learning curve
        /// </summary>
		[HttpPost("/score")]
		public async Task<ActionResult> Score([FromBody, Required] Greeting greeting, [FromServices] DaprClient daprClient, [FromServices] IScoringService scoringService)
		{
			greeting.Score = await scoringService.Score(greeting.Email);
			logger.LogInformation("{Name} score: {Score}", greeting.Name, greeting.ScorePercent);

			var elementInstanceKey = long.Parse(Request.Headers[Headers.ZeebeElementInstanceKey]);
			await daprClient.InvokeBindingAsync<SetVariablesRequest, SetVariablesResponse>(Bindings.ZeebeCommand, Commands.SetVariables,
				new SetVariablesRequest(elementInstanceKey, greeting, false));

			return Ok();
		}

		/// <summary>
        /// Assess person's score
        /// </summary>
		[HttpPost("/assess")]
		public async Task<ActionResult> Assess([FromBody, Required] Greeting greeting, [FromServices] DaprClient daprClient, [FromServices] IScoringService scoringService)
		{
			if (greeting.Score < 0.2)
			{
				greeting.Comments = $"Skills of {greeting.Name} are not sufficient. Score: {greeting.ScorePercent}";
				logger.LogWarning(greeting.Comments);
			}

			var elementInstanceKey = long.Parse(Request.Headers[Headers.ZeebeElementInstanceKey]);
			await daprClient.InvokeBindingAsync<SetVariablesRequest, SetVariablesResponse>(Bindings.ZeebeCommand, Commands.SetVariables,
				new SetVariablesRequest(elementInstanceKey, greeting, false));

			return Ok();
		}

        /// <summary>
        /// Greet person
        /// </summary>
        [HttpPost("/greet")]
		public async Task<ActionResult> Greet([FromBody] Greeting greeting, [FromServices] DaprClient daprClient)
		{
			// send email
			var body = ComposeGreetingMessage(greeting);
			var metadata = new Dictionary<string, string>
			{
				["emailFrom"] = "noreply@incredible.inc",
				["emailTo"] = greeting.Email,
				["subject"] = $"Greetings for {greeting.Name}!"
			};

			await daprClient.InvokeBindingAsync(Bindings.SendEmail, "create", body, metadata);

			// simulate business error if scored less than 20%
			if (greeting.Score < 0.2)
			{
				var jobKey = long.Parse(Request.Headers[Headers.ZeebeJobKey]);
				var throwErrorRequest = new ThrowErrorRequest(jobKey, "GreetingError", "Insufficient score.");
				await daprClient.InvokeBindingAsync<ThrowErrorRequest, ThrowErrorResponse>(Bindings.ZeebeCommand, Commands.ThrowError, throwErrorRequest);
			}

			return Ok();
		}

        /// <summary>
        /// Send email to admin
        /// </summary>
        [HttpPost("/send-email-admin")]
		public async Task<ActionResult> SendEmailAdmin([FromBody, Required] Greeting greeting, [FromServices] DaprClient daprClient)
		{
			var jobKey = long.Parse(Request.Headers["X-Zeebe-Job-Key"]);
			var body = greeting.Comments;
			var metadata = new Dictionary<string, string>
			{
				["emailFrom"] = "noreply@incredible.inc",
				["emailTo"] = "admin@incredible.inc",
				["subject"] = $"Greeter job {jobKey} failed."
			};

			await daprClient.InvokeBindingAsync(Bindings.SendEmail, "create", body, metadata);

			return Ok();
		}

        private static string ComposeGreetingMessage(Greeting greeting) => $"{greeting.DesiredGreeting} {greeting.Name}! Your score: {greeting.ScorePercent}";

	}
}
