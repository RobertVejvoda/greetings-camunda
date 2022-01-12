using Dapr.Client;
using greetings_camunda.Model;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using greetings_camunda.Command;
using Microsoft.Toolkit.HighPerformance;
using System.Collections.Generic;
namespace greetings_camunda.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class CommandController : ControllerBase
    {
        private readonly DaprClient daprClient;

        public CommandController(DaprClient daprClient)
        {
            this.daprClient = daprClient;
        }

        [HttpGet(Commands.Topology)]
        public async Task<TopologyResponse> Topology()
        {
            return await daprClient.InvokeBindingAsync<object, TopologyResponse>("zeebe-command", Commands.Topology, new {});
        }

        [HttpPost(Commands.CreateInstance)]
        public async Task<ActionResult> CreateInstance([FromBody] CreateInstanceRequest request)
        {
            if (request.BpmnProcessId == null && request.ProcessDefinitionKey == null)
            {
                return BadRequest(new { error = "Either a bpmnProcessId or a processDefinitionKey must be given" });
            }

            var result = await daprClient.InvokeBindingAsync<CreateInstanceRequest, CreateInstanceResponse>(
                "zeebe-command", Commands.CreateInstance, request);

            return Ok(result);
        }

        [HttpPost(Commands.PublishMessage)]
        public async Task<PublishMessageResponse> PublishMessage([FromBody] PublishMessageRequest request)
        {
            return await daprClient.InvokeBindingAsync<PublishMessageRequest, PublishMessageResponse>(
                "zeebe-command", Commands.PublishMessage, request);
        }

        [HttpPost(Commands.ActivateJobs)]
        public async Task<IList<ActivatedJob>> PublishMessage([FromBody] ActivateJobsRequest request)
        {
            return await daprClient.InvokeBindingAsync<ActivateJobsRequest, IList<ActivatedJob>>(
                "zeebe-command", Commands.ActivateJobs, request);
        }
    }
}