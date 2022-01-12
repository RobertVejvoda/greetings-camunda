using Dapr.Client;
using greetings_camunda.Command;
using greetings_camunda.Extensions;
using greetings_camunda.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
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
		/// POST from Zeebe Broker should invoke this method through zeebe jobworker output binding
		/// </summary>
		[HttpPost("/greet")]
		public async Task<ActionResult> Greet([FromBody] GreetingRequest request, [FromServices] DaprClient daprClient)
		{
			logger.LogZeebeHeaders(Request);

			if (DateTime.Now.Millisecond % 2 == 0)
			{
			  var jobKey = long.Parse(Request.Headers["X-Zeebe-Job-Key"]);
			  var throwError = new ThrowErrorRequest(jobKey, "GreetingError", "No greeting this time!");
			  var response = await daprClient.InvokeBindingAsync<ThrowErrorRequest, ThrowErrorResponse>("zeebe-command", "throw-error", throwError);

			  return Ok(response);
			}

			var name = request?.Name ?? throw new ArgumentNullException(nameof(request));
			var greet = greetingService.Greet(name);

			logger.LogInformation(greet);

			return Ok(new GreetingResponse(request.GreetingId, request.Name, request.Email, greet));
		}

		/// <summary>
		/// POST from Zeebe Broker should invoke this method through zeebe jobworker output binding
		/// </summary>
		[HttpPost("/send-email")]
		public async Task<ActionResult> SendEmail([FromBody] GreetingResponse greeting, [FromServices] DaprClient daprClient)
		{
			var body = greeting.Greeting;
			var metadata = new Dictionary<string, string>
			{
				["emailFrom"] = "noreply@incredible.inc",
				["emailTo"] = greeting.Email,
				["subject"] = $"Hey {greeting.Name}, learn new greeting every day!"
			};

			await daprClient.InvokeBindingAsync("sendmail", "create", body, metadata);

			return Ok();
		}

		/// <summary>
		/// POST from Zeebe Broker should invoke this method through zeebe jobworker output binding
		/// </summary>
		[HttpPost("/send-email-admin")]
		public async Task<ActionResult> SendEmailAdmin([FromBody] GreetingRequest request, [FromServices] DaprClient daprClient)
		{
			var body = $"Sadly we couldn't greet {request.Name}!";
			var metadata = new Dictionary<string, string>
			{
				["emailFrom"] = "noreply@incredible.inc",
				["emailTo"] = "admin@incredible.inc",
				["subject"] = $"Greeting failed!"
			};

			await daprClient.InvokeBindingAsync("sendmail", "create", body, metadata);

			return Ok();
		}
	}
}
