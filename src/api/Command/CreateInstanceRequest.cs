using System.Collections.Generic;

namespace greetings_camunda.Command
{
    public record CreateInstanceRequest(
        string BpmnProcessId,
        long? ProcessDefinitionKey,
        int? Version,
        Dictionary<string, string> Variables);
}