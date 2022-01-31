using System.ComponentModel.DataAnnotations;

namespace greetings_camunda.Command
{
    public record SetVariablesRequest(
        [Required] long? ElementInstanceKey,
        [Required] object Variables,
        bool? Local);
}