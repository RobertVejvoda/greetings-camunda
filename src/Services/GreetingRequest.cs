using System.ComponentModel.DataAnnotations;

namespace greetings_camunda.Services
{
    public record GreetingRequest([Required] string GreetingId, [Required] string Name, [Required] string Email);
	
}
