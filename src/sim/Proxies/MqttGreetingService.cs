using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace sim.Proxies
{

    internal class MqttGreetingService : IGreetingService
	{
		private readonly IMqttClient mqttClient;
		private readonly IMqttClientOptions mqttClientOptions;
		private readonly ILogger logger;

		public MqttGreetingService(ILogger<MqttGreetingService> logger)
		{
			// connect mqtt broker
			this.logger = logger;
			var mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST") ?? "localhost";
			var mqttPort = Environment.GetEnvironmentVariable("MQTT_PORT") ?? "1883";

			var mqttFactory = new MqttFactory();
			this.mqttClient = mqttFactory.CreateMqttClient();
			this.mqttClientOptions = mqttFactory.CreateClientOptionsBuilder()
				.WithProtocolVersion(MQTTnet.Formatter.MqttProtocolVersion.V500)
				.WithTcpServer(mqttHost, int.Parse(mqttPort))
				.WithCleanSession()
				.Build();

			mqttClient.ConnectAsync(mqttClientOptions).GetAwaiter().GetResult();
		}

		public async Task GreetAsync(GreetingRequest request, CancellationToken cancellationToken = default)
		{
			var eventJson = JsonConvert.SerializeObject(request);

			var message = new MqttApplicationMessageBuilder()
				.WithTopic("camunda/greeting-requested")
				.WithContentType("application/json")
				.WithPayload(eventJson)
				.WithRetainFlag(false)
				.WithAtMostOnceQoS()
				.Build();

			logger.LogInformation(message.ConvertPayloadToString());

			await mqttClient.PublishAsync(message, cancellationToken);
		}
	}
}
