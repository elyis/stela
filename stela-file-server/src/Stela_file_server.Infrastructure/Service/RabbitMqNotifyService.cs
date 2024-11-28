using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Stela_file_server.Core.Enums;
using Stela_file_server.Core.IService;

namespace Stela_file_server.Infrastructure.Service
{
    public class RabbitMqNotifyService : INotifyService
    {
        private readonly string _hostname;
        private readonly string _username;
        private readonly string _password;

        private readonly string _profileImageQueue;
        private readonly string _additionalServiceImageQueue;
        private readonly string _memorialImageQueue;
        private readonly string _portfolioMemorialImageQueue;
        private readonly string _materialImageQueue;

        public RabbitMqNotifyService(
            string hostname,
            string username,
            string password,
            string profileImageQueue,
            string additionalServiceImageQueue,
            string memorialImageQueue,
            string portfolioMemorialImageQueue,
            string materialImageQueue)
        {
            _hostname = hostname;
            _username = username;
            _password = password;

            _profileImageQueue = profileImageQueue;
            _additionalServiceImageQueue = additionalServiceImageQueue;
            _memorialImageQueue = memorialImageQueue;
            _portfolioMemorialImageQueue = portfolioMemorialImageQueue;
            _materialImageQueue = materialImageQueue;
        }

        public void Publish<T>(T message, ContentUploaded content)
        {
            string queueName = GetQueueName(content);

            var factory = new ConnectionFactory()
            {
                HostName = _hostname,
                UserName = _username,
                Password = _password
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: queueName,
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: properties,
                                 body: body);
        }

        private string GetQueueName(ContentUploaded content)
        {
            return content switch
            {
                ContentUploaded.ProfileImage => _profileImageQueue,
                ContentUploaded.AdditionalServiceImage => _additionalServiceImageQueue,
                ContentUploaded.MemorialImage => _memorialImageQueue,
                ContentUploaded.PortfolioMemorialImage => _portfolioMemorialImageQueue,
                ContentUploaded.MaterialImage => _materialImageQueue,
            };
        }
    }
}