using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using ClamShell.MessageBus.Models;

namespace ClamShell.MessageBus
{
    public class Publisher<T> : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queue;

        public Publisher(string queue)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _queue = queue;

            _channel.QueueDeclare(
                queue: _queue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public void Publish(TransferModel<T> data)
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));

            _channel.BasicPublish(
                exchange: "",
                routingKey: _queue,
                basicProperties: null,
                body: body);

            System.Console.WriteLine($"Sent message to queue '{_queue}'");
        }

        public void Dispose()
        {
            _connection?.Dispose();
            _channel?.Dispose();
        }
    }
}
