using RabbitMQ.Client;
using System.Text.Json;
using System.Text;
using ClamShell.MessageBus.Models;

namespace ClamShell.MessageBus
{
    public class Publisher<T> : IDisposable
    {
        private const int maxAttempts = 10;
        private const int delayMilliseconds = 2000;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queue;

        public Publisher(string queue)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    _connection = factory.CreateConnection();
                    break; // Exit the loop if the connection is successful
                }
                catch (Exception ex)
                {
                    if (attempt == maxAttempts)
                    {
                        throw; // Rethrow the exception if the maximum number of attempts is reached
                    }
                    Console.WriteLine($"Attempt {attempt} failed: {ex.Message}");
                    Console.WriteLine($"Retrying in {delayMilliseconds / 1000} seconds...");
                    Thread.Sleep(delayMilliseconds); // Wait before retrying
                }
            }
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
