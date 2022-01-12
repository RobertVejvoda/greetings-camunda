using System.ComponentModel.DataAnnotations;

namespace greetings_camunda.Services
{
	public record GreetingResponse([Required] string GreetingId, [Required] string Name, [Required] string Email, [Required] string Greeting);
}
