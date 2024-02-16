using ClamShell.MessageBus.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ClamShell.MessageBus
{
    public class Consumer<T> : IDisposable
    {
        private const int maxAttempts = 10;
        private const int delayMilliseconds = 2000;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private string _queue;

        public event EventHandler<TransferModel<T>> MessageReceived;

        public Consumer(string queue)
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

            _channel.QueueDeclare(queue: _queue,
                         durable: true,
                         exclusive: false,
                         autoDelete: false,
                         arguments: null);

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                var transferModel = Newtonsoft.Json.JsonConvert.DeserializeObject<TransferModel<T>>(message);

                OnMessageReceived(transferModel);
            };

            _channel.BasicConsume(queue: _queue,
                                  autoAck: true,
                                  consumer: consumer);
        }
        protected virtual void OnMessageReceived(TransferModel<T> e)
        {
            MessageReceived?.Invoke(this, e);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }

    }
}
