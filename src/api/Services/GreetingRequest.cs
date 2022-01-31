using System;
using System.ComponentModel.DataAnnotations;

namespace greetings_camunda.Services
{
    public class GreetingRequest
    {
        [Required]
        public Guid GreetingId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        public string Greeting { get; set; }
        public string GreetingMessage { get; set; }

        public override string ToString()
            => $"{GreetingId}, {Name}, {Email}";
    }
}
