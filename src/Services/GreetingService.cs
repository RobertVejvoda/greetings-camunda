using System;

namespace greetings_camunda.Services
{
    public interface IGreetingService
	{
		string Greet(string name);
	}

	internal class GreetingService : IGreetingService
	{
		private readonly string[] greets = new[] 
		{
			"mirëmëngjes", "good morning", "dobar dan", "goddag", "bonan tagon", "tere päevast", "hyvää päivää",
			"bonjour", "buongiorno", "今日は", "laba diena", "labdien", "Guten Tag", "grüezi", "dzień dobry",
			"boa tarde", "lačho ďives", "здравствуйте", "καλημέρα", "buenos días", "god dag", "dobrý den", "dobrý deň" 
		};

		public string Greet(string name)
		{
			var rng = new Random();
			var index = rng.Next(0, greets.Length); // min >= index < max
			return string.Concat(greets[index], " ", name);
		}
	}
	
}
