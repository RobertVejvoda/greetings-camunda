using Microsoft.Extensions.Logging;
using sim.Proxies;
using System.Threading.Tasks;

namespace sim
{
    internal class Program
	{
		static async Task Main(string[] args)
		{
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            
            var simulator = new GreetingSimulator(
                //new DaprPubSubService(),
                // new DebugGreetingService(loggerFactory.CreateLogger<DebugGreetingService>()),
                new MqttGreetingService(loggerFactory.CreateLogger<MqttGreetingService>()),
                loggerFactory.CreateLogger<GreetingSimulator>());

            var processRecords = args.Length > 0 ? int.Parse(args[0]) : -1;

            await simulator.StartAsync(processRecords);
        }
    }
}
