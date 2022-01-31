using System;

namespace greetings_camunda.Services
{
    public interface IGreetingService
	{
		string DecideGreeting(TimeOnly time);
		string Greet(string greeting, string name);
	}

	internal class GreetingService : IGreetingService
	{
		private readonly string[] morningGreets = new[] { "Dobré ráno", "Bonjour", "Good morning", "Добрый день", "Καλημέρα" };
		private readonly string[] afternoonGreets = new[] { "Dobré odpoledne", "Bonne après-midi", "Good afternoon", "Добрый день", "καλό απόγευμα" };
		private readonly string[] eveningGreets = new[] { "Dobrý večer", "Bonsoir", "Good evening", "Добрый вечер", "Καλό απόγευμα" };

        public string DecideGreeting(TimeOnly time)
        {
            return time.Hour switch
            {
                < 12 => morningGreets[Random.Shared.Next(morningGreets.Length)],
                < 18 => afternoonGreets[Random.Shared.Next(morningGreets.Length)],
                _ => eveningGreets[Random.Shared.Next(morningGreets.Length)]
            };
        }

        public string Greet(string greeting, string name)
		{
			return $"{greeting} {name}!";
		}
	}
	
}
