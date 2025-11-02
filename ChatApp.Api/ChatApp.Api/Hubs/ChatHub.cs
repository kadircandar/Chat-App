using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using System.Security.Claims;

namespace ChatApp.Api.Hubs
{
    public class ChatHub : Hub
    {
        // Kullanıcı bağlantılarını takip etmek için
        public static readonly ConcurrentDictionary<string, string> OnlineUsers = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Kullanıcı bağlandığında çalışır
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                OnlineUsers[Context.ConnectionId] = userId;

                // Tüm clientlara bildir
                await Clients.All.SendAsync(HubEvents.UserConnected, userId);
                await Clients.All.SendAsync(HubEvents.UpdateOnlineFriends, OnlineUsers.Values.Distinct().ToList());
            }

            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Kullanıcı ayrıldığında çalışır
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (OnlineUsers.TryRemove(Context.ConnectionId, out var userId))
            {
                await Clients.All.SendAsync(HubEvents.UserDisconnected, userId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Kullanıcıya özel mesaj gönderme
        /// </summary>
        public async Task SendMessage(string receiverUserId, string message)
        {
            // Hedef kullanıcının bağlantılarını bul
            foreach (var kvp in OnlineUsers)
            {
                if (kvp.Value == receiverUserId)
                {
                    await Clients.Client(kvp.Key).SendAsync(HubEvents.ReceiveMessage, Context.ConnectionId, message);
                }
            }

            // Gönderen kullanıcıya da geri bildir
            await Clients.Caller.SendAsync(HubEvents.ReceiveMessage, receiverUserId, message);
        }

        /// <summary>
        /// Online kullanıcıları listeleme
        /// </summary>
        public Task GetOnlineUsers()
        {
            var users = OnlineUsers.Values;
            return Clients.Caller.SendAsync(HubEvents.OnlineUsers, users);
        }
    }
}
