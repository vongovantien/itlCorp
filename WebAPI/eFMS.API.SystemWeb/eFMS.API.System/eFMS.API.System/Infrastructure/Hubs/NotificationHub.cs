using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using eFMS.IdentityServer.DL.UserManager;
using System;
using System.Collections.Generic;

namespace eFMS.API.System.Infrastructure.Hubs
{
    [Authorize]
    public class NotificationHub : Hub<IHubClientNotification>
    {
        readonly ICurrentUser currentUser;
        public static HashSet<string> ConnectedIds = new HashSet<string>();

        public NotificationHub(ICurrentUser currentUser)
        {
            this.currentUser = currentUser;
        }

        public HashSet<string> GetConnectionIds()
        {
            return ConnectedIds;
        }

        public string GetConnectionId()
        {
            return Context.ConnectionId;
        }

        public async Task SendMessageToClient(string connectionId, string message)
        {
            await Clients.Client(connectionId).SendMessageToClient(connectionId, message);
        }

        public async Task SendMessageToAllClient(string msg)
        {
            await Clients.All.SendMessageToAllClient(msg);
        }

        public async Task BroadCastMessage(string msg)
        {
            Clients.All.BroadCastMessage(msg);
        }

        public override async Task OnConnectedAsync()
        {
            Console.WriteLine("---------------------------------------------------------------------------------------");
            Console.WriteLine(Context.ConnectionId + " Connected ");
            Console.WriteLine("---------------------------------------------------------------------------------------");
            ConnectedIds.Add(Context.ConnectionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            Console.WriteLine("---------------------------------------------------------------------------------------");
            Console.WriteLine(Context.ConnectionId + " Diconnected ");
            Console.WriteLine("---------------------------------------------------------------------------------------");
            ConnectedIds.Remove(Context.ConnectionId);
            await base.OnConnectedAsync();
        }
    }

    public interface IHubClientNotification
    {
        Task SendMessageToClient(string connectionId, string msg);
        Task SendMessageToAllClient(string message);
        Task BroadCastMessage(string message);

    }
}
