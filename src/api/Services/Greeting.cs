using System;

namespace greetings_camunda.Services
{
    public class Greeting
    {
        public Greeting(Guid greetingId, string name, string email, double score, string desiredGreeting, string comments)
        {
            this.GreetingId = greetingId;
            this.Name = name;
            this.Email = email;
            this.Score = score;
            this.DesiredGreeting = desiredGreeting;
            this.Comments = comments;
        }

        public Guid GreetingId { get; private set; }
        public string Name { get; private set; }
        public string Email { get; private set; }
        public double Score { get; set; }
        public string DesiredGreeting { get; set; }
        public string Comments { get; set; }

        public string ScorePercent => $"{Math.Round(Score*100, 2)}%";
    }
}