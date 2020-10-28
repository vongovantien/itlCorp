using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using eFMS.IdentityServer.DL.UserManager;
namespace eFMS.API.System.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        ICurrentUser currentUser;
        public NotificationHub(ICurrentUser currentUser)
        {
            this.currentUser = currentUser;
            hubContext = _hubContext;
    }

        public string GetConnectionId()
        {
            return Context.ConnectionId ;
        }
        public object GetConnectionUser()
        {
            var user = new
            {
                currentUser.UserName,
                UserId = currentUser.UserID
            };
            return user;
        }

        public async Task SendToUser(string userId, string data)
        {
            await Clients.User(userId).SendAsync("onChangeNotificationUser", data);
        }

        public async Task SendToConnection(string connectionId, string data)
        {
            await Clients.Client(connectionId).SendAsync("onChangeNotificationConnection", data);
        }
    }
}
