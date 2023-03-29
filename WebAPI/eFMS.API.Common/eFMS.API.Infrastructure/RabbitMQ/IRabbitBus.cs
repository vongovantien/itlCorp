using System;
using System.Threading.Tasks;

namespace eFMS.API.Infrastructure.RabbitMQ
{
    public interface IRabbitBus
    {
        Task SendAsync<T>(string queue, T message);
        Task SendAsync<T>(string exchange, string queue, T message);
        Task ReceiveAsync<T>(string queue, Action<T> onMessage);
        Task ReceiveAsync<T>(string exchange, string queue, Action<T> onMessage);
        Task ReceiveAsync<T>(string exchange, string queue, Action<T> onMessage, int batchSize, int maxMessagesInFlight);
        Task ReceiveAsync<T>(string exchange, string queue, Action<T> onMessage, TimeSpan interval);

    }
}
