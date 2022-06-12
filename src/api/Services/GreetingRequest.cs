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

        public double Score {get; set; }

        public double ScorePercent => Math.Round(Score*100,2);

        public string Greeting { get; set; }

        public override string ToString()
            => $"ID: {GreetingId}\r\nName: {Name}\r\nEmail: {Email}\r\nScore: {ScorePercent}";
    }
}
