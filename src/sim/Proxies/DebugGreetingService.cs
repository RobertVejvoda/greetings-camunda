using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace sim.Proxies
{
    public class DebugGreetingService : IGreetingService
	{
        private readonly ILogger logger;

        public DebugGreetingService(ILogger<DebugGreetingService> logger)
        {
            this.logger = logger;
        }

		public Task GreetAsync(GreetingRequest request, CancellationToken cancellationToken)
		{
			logger.LogInformation(request.ToString());
			Debug.WriteLine(request.ToString());
			return Task.CompletedTask;
		}
	}
}
