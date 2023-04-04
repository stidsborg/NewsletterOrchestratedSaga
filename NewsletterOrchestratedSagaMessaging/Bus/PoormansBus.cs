using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NewsletterOrchestratedSagaMessaging.Bus;

public class PoormansBus
{
    private readonly IConnection _connection;

    public PoormansBus()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };
        _connection = factory.CreateConnection();
    }

    public IModel Subscribe(string queue, Func<object, Task> handler, bool deleteExistingQueue)
    {
        var channel = _connection.CreateModel();

        if (deleteExistingQueue)
            channel.QueueDelete(queue);

        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: false);
        channel.QueueBind(queue, exchange: "amq.topic", routingKey: "#");
        
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var messageJson = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;
            var messageType = Type.GetType(routingKey, throwOnError: true);
            var message = JsonSerializer.Deserialize(messageJson, messageType!);
            handler(message!).GetAwaiter().GetResult();
            channel.BasicAck(ea.DeliveryTag, multiple: false);
        };
        channel.BasicConsume(
            queue,
            autoAck: false,
            consumer: consumer
        );

        return channel;
    }

    public void Publish<T>(T message)
    {
        using var channel = _connection.CreateModel();
        var json = JsonSerializer.Serialize(message);
        var jsonBytes = Encoding.UTF8.GetBytes(json);
        channel.BasicPublish(exchange: "amq.topic", routingKey: typeof(T).FullName, body: jsonBytes);
    }
}