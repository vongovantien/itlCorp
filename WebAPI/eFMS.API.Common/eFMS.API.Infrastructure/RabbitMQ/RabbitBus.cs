using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading.Tasks;

namespace eFMS.API.Infrastructure.RabbitMQ
{
    public class RabbitBus : IRabbitBus
    {
        private readonly IModel _channel;

        internal RabbitBus(IModel channel)
        {
            _channel = channel;
        }
        public async Task SendAsync<T>(string queue, T message)
        {
            await Task.Run(() =>
            {
                _channel.QueueDeclare(queue, true, false, false);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                var output = JsonConvert.SerializeObject(message);
                _channel.BasicPublish(string.Empty, queue, properties, Encoding.UTF8.GetBytes(output));
            });
        }

        public async Task SendAsync<T>(string exchange, string queue, T message)
        {
            await Task.Run(() =>
            {
                _channel.ExchangeDeclare(exchange, "direct", true, false);
                _channel.QueueDeclare(queue, true, false, false);

                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;

                var output = JsonConvert.SerializeObject(message);
                _channel.BasicPublish(exchange, queue, body: Encoding.UTF8.GetBytes(output));
            });
        }
        public async Task ReceiveAsync<T>(string queue, Action<T> onMessage)
        {
            _channel.QueueDeclare(queue, true, false, false);
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (s, e) =>
            {
                var jsonSpecified = Encoding.UTF8.GetString(e.Body.Span);
                var item = JsonConvert.DeserializeObject<T>(jsonSpecified);
                onMessage(item);
                _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                await Task.Yield();
            };
            _channel.BasicConsume(queue, autoAck: false, consumer: consumer);
            await Task.Yield();
        }

        public async Task ReceiveAsync<T>(string exchange, string queue, Action<T> onMessage)
        {
            _channel.ExchangeDeclare(exchange, "direct", true, false);
            _channel.QueueDeclare(queue, true, false, false);
            _channel.QueueBind(queue, exchange, queue);
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (s, e) =>
            {
                var jsonSpecified = Encoding.UTF8.GetString(e.Body.Span);
                var item = JsonConvert.DeserializeObject<T>(jsonSpecified);
                onMessage(item);
                _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                await Task.Yield();
            };
            _channel.BasicConsume(queue, autoAck: false, consumer: consumer);
            await Task.Yield();
        }

        public async Task ReceiveAsync<T>(string exchange, string queue, Action<T> onMessage, TimeSpan interval)
        {
            _channel.ExchangeDeclare(exchange, "direct", true, false);
            _channel.QueueDeclare(queue, true, false, false);
            _channel.QueueBind(queue, exchange, queue);
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (s, e) =>
            {
                var jsonSpecified = Encoding.UTF8.GetString(e.Body.Span);
                var item = JsonConvert.DeserializeObject<T>(jsonSpecified);
                onMessage(item);
                _channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
                await Task.Delay(interval);
            };
            _channel.BasicConsume(queue, autoAck: false, consumer: consumer);
            await Task.Yield();

        }
    }
}
