using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using eFMS.IdentityServer.DL.UserManager;
using System;

namespace eFMS.API.System.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        ICurrentUser currentUser;
        public NotificationHub(ICurrentUser currentUser)
        {
            this.currentUser = currentUser;
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

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("---------------------------------------------------------------------------------------");
            Console.WriteLine(Context.ConnectionId + " Connected ");
            Console.WriteLine("---------------------------------------------------------------------------------------");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            Console.WriteLine("---------------------------------------------------------------------------------------");
            Console.WriteLine(Context.ConnectionId + " Diconnected ");
            Console.WriteLine("---------------------------------------------------------------------------------------");
            await base.OnConnectedAsync();
        }
    }
}
