using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using sim.Proxies;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace sim
{
    public class GreetingSimulator
	{
        private readonly ILogger logger;
        private readonly IGreetingService service;

		public GreetingSimulator(IGreetingService service, ILogger<GreetingSimulator> logger)
		{
            this.logger = logger;
            this.service = service;
		}

		public async Task StartAsync(int processRecords = -1, CancellationToken cancellationToken = default) 
		{
			logger.LogInformation($"Starting Greeter simulation.");

			var config = new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				HasHeaderRecord = true,
				Delimiter = ";"
			};

			var random = new Random();
			
			try
			{
				var counter = 0;
				using var reader = new StreamReader("names.csv");
				using var csv = new CsvReader(reader, config);
				await foreach (var greetingName in csv.GetRecordsAsync<GreetingName>(cancellationToken))
				{
					if (counter >= processRecords && processRecords != -1)
						break;

					await service.GreetAsync(new GreetingRequest
												{
													GreetingId = Guid.NewGuid(),
													Email = greetingName.Email,
													Name = greetingName.FullName
												}, cancellationToken);
					await Task.Delay(250, cancellationToken);

					counter++;
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex.ToString());
				if (ex is TaskCanceledException)
					throw;
			}

			logger.LogInformation($"Greeter simulation finished.");

		}
	}
}
