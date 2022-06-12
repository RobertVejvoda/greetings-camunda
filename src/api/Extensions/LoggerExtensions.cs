using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace greetings_camunda.Extensions
{
	public static class LoggerExtensions
	{
		public static void LogZeebeHeaders(this ILogger logger, HttpRequest httpRequest)
		{
      logger.LogInformation("Zeebe JobKey: {Value}", httpRequest.Headers["X-Zeebe-Job-Key"]);
      logger.LogInformation("Zeebe JobType: {Value}", httpRequest.Headers["X-Zeebe-Job-Type"]);
      logger.LogInformation("Zeebe ProcessInstanceKey: {Value}", httpRequest.Headers["X-Zeebe-Process-Instance-Key"]);
      logger.LogInformation("Zeebe BpmnProcessId: {Value}", httpRequest.Headers["X-Zeebe-Bpmn-Process-Id"]);
      logger.LogInformation("Zeebe ProcessDefinitionVersion: {Value}", httpRequest.Headers["X-Zeebe-Process-Definition-Version"]);
      logger.LogInformation("Zeebe ProcessDefinitionKey: {Value}", httpRequest.Headers["X-Zeebe-Process-Definition-Key"]);
      logger.LogInformation("Zeebe ElementId: {Value}", httpRequest.Headers["X-Zeebe-Element-Id"]);
      logger.LogInformation("Zeebe ElementInstanceKey: {Value}", httpRequest.Headers["X-Zeebe-Element-Instance-Key"]);
      logger.LogInformation("Zeebe Worker: {Value}", httpRequest.Headers["X-Zeebe-Worker"]);
      logger.LogInformation("Zeebe Retries: {Value}", httpRequest.Headers["X-Zeebe-Retries"]);
      logger.LogInformation("Zeebe Deadline: {Value}", httpRequest.Headers["X-Zeebe-Deadline"]);
      logger.LogInformation("Zeebe Custom ErrorCode: {Value}", httpRequest.Headers["X-Custom-ErrorCode"]);
      logger.LogInformation("Zeebe Custom ErrorMessage: {Value}", httpRequest.Headers["X-Custom-ErrorMessage"]);
    }

	}
}
