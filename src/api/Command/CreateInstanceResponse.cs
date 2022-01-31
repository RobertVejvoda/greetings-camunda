namespace greetings_camunda.Command
{
    public record CreateInstanceResponse(
        long? ProcessDefinitionKey,
        string BpmnProcessId,
        int? Version,
        long? ProcessInstanceKey);
}