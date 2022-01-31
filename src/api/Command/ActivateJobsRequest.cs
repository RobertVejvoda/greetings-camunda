using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace greetings_camunda.Command
{
    public record ActivateJobsRequest(
        [Required] string JobType,
        [Required] int? MaxJobsToActivate,
        string Timeout,
        string WorkerName,
        IList<string> FetchVariables);
}