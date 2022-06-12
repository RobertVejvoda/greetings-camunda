using System;
using System.Threading.Tasks;

namespace greetings_camunda.Services
{
    public interface IScoringService
    {
        Task<double> Score(string email);
    }

    internal class ScoringService : IScoringService
    {
        public Task<double> Score(string email) 
        {
            return Task.FromResult(Random.Shared.NextDouble());
        }
    }
}
