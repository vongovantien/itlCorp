using System.Threading.Tasks;

namespace eFMS.API.System.Infrastructure.Hubs
{
    public interface IHubNotification
    {
        Task SendToUser(string userId, string data);
        Task SendToConnection(string connectionId, string data);
    }

}
