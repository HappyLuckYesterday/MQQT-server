using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Text;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        var factory = new MqttFactory();
        var mqttClient = factory.CreateMqttClient();

        var options = new MqttClientOptionsBuilder()
            .WithClientId("Client1")
            .WithTcpServer("localhost")
            .WithCleanSession()
            .Build();

        mqttClient.UseConnectedHandler(async e =>
        {
            Console.WriteLine("Connected successfully with MQTT Brokers.");
            await mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic("status/client-id").Build());

            Console.WriteLine("Subscribed");
        });

        mqttClient.UseDisconnectedHandler(async e =>
        {
            Console.WriteLine("Disconnected from MQTT Brokers.");
            await Task.Delay(TimeSpan.FromSeconds(5));

            try
            {
                await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken
            }
            catch
            {
                Console.WriteLine("Reconnecting to MQTT Brokers failed.");
            }
        });

        mqttClient.UseApplicationMessageReceivedHandler(e =>
        {
            Console.WriteLine("### RECEIVED APPLICATION MESSAGE ###");
            Console.WriteLine($"+ Topic = {e.ApplicationMessage.Topic}");
            Console.WriteLine($"+ Payload = {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
            Console.WriteLine($"+ QoS = {e.ApplicationMessage.QualityOfServiceLevel}");
            Console.WriteLine($"+ Retain = {e.ApplicationMessage.Retain}");
            Console.WriteLine();
        });

        await mqttClient.ConnectAsync(options, CancellationToken.None); // Since 3.0.5 with CancellationToken

        Console.WriteLine("Press any key to exit.");
        Console.ReadLine();

        await mqttClient.DisconnectAsync();
    }
}