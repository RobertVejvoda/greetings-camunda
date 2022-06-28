using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace sim.Proxies
{

    internal class MqttGreetingService : IGreetingService
	{
		private readonly IMqttClient mqttClient;
		private readonly IMqttClientOptions mqttClientOptions;
		private readonly ILogger logger;
		private readonly JsonSerializerSettings jsonSerializerSettings;


		public MqttGreetingService(ILogger<MqttGreetingService> logger)
		{
			// connect mqtt broker
			this.logger = logger;
			this.jsonSerializerSettings = new JsonSerializerSettings 
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver()
			};
			var mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST") ?? "localhost";
			var mqttPort = Environment.GetEnvironmentVariable("MQTT_PORT") ?? "1883";

			var mqttFactory = new MqttFactory();
			this.mqttClient = mqttFactory.CreateMqttClient();
			this.mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
				.WithTcpServer(mqttHost, int.Parse(mqttPort))
				.WithCleanSession()
				.Build();

			mqttClient.ConnectAsync(mqttClientOptions).GetAwaiter().GetResult();
		}

		public async Task GreetAsync(GreetingRequest request, CancellationToken cancellationToken = default)
		{
			var eventJson = JsonConvert.SerializeObject(request, Formatting.None, jsonSerializerSettings); 

			var message = new MqttApplicationMessageBuilder()
				.WithTopic("camunda/greeting-requested")
				.WithPayload(Encoding.UTF8.GetBytes(eventJson))
				.WithAtMostOnceQoS()
				.Build();

			logger.LogInformation(message.ConvertPayloadToString());

			await mqttClient.PublishAsync(message, cancellationToken);
		}
	}
}
