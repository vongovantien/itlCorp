using eFMS.API.Common.Helpers;
using RabbitMQ.Client;
using System;

namespace eFMS.API.Infrastructure.RabbitMQ
{
    public class RabbitMQHelper
    {
        private static ConnectionFactory _factory;
        private static IConnection _connection;
        private static IModel _channel;

        public static IRabbitBus CreateBus(
            string hostName,
            int hostPort,
            string virtualHost,
            string username,
            string password)
        {
            _factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = 5672,
                VirtualHost = virtualHost,
                UserName = username,
                Password = password,
                DispatchConsumersAsync = true
            };
            try
            {
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();

                return new RabbitBus(_channel);
            }
            catch (Exception ex)
            {
                new LogHelper("RabbitMQ_Log", ex.ToString());
                throw;
            }
        }
    }
}
