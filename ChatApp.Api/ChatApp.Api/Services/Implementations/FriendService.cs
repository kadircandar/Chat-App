using ChatApp.Api.Data;
using ChatApp.Api.DTOs;
using ChatApp.Api.DTOs.Friends;
using ChatApp.Api.Hubs;
using ChatApp.Api.Models;
using ChatApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Services.Implementations
{
    public class FriendService : IFriendService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hub;

        public FriendService(AppDbContext context, IHubContext<ChatHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public List<GetFriendsDto> GetFriends(string userId)
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            var friends = _context.FriendRequests
                .Where(f => (f.SenderUserId == userId || f.ReceiverUserId == userId) && f.Status == FriendRequestStatus.Accepted)
                .Select(f => new GetFriendsDto
                {
                    UserId = f.ReceiverUserId == userId ? f.SenderUserId : f.ReceiverUserId,
                    Username = f.ReceiverUserId == userId ? f.Sender.Username : f.Receiver.Username
                })
                .ToList();

            foreach (var user in friends)
            {
                var lastMessage = _context.Messages
                    .Where(m => (m.SenderUserId == user.UserId && m.ReceiverUserId == userId) ||
                    (m.SenderUserId == userId && m.ReceiverUserId == user.UserId))
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => new
                    {
                        Time = m.SentAt,
                        Message = m.ContentEncrypted,
                        SenderUserId = m.SenderUserId
                    })
                    .FirstOrDefault();

                user.IsOnline = ChatHub.OnlineUsers.Any(o => o.Value.Equals(user.UserId.ToString(), StringComparison.OrdinalIgnoreCase));
                user.UnreadCount = _context.Messages.Count(m => m.ReceiverUserId == userId && m.SenderUserId == user.UserId && !m.IsRead);
                user.Initials = user.Username.Substring(0, 1).ToUpper();
                user.Time = lastMessage?.Time;
                user.LastMessage = lastMessage?.Message ?? string.Empty;
                user.SenderUserId = lastMessage?.SenderUserId ?? string.Empty;
            }

            return friends;
        }

        public List<GetPendingRequestsDto> GetPendingRequests(string userId)
        {
            _context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return _context.FriendRequests
               .Where(r => r.ReceiverUserId == userId && r.Status == FriendRequestStatus.Pending)
               .Select(r => new GetPendingRequestsDto
               {
                   SenderUsername = r.Sender.Username,
                   SenderUserId = userId,
                   Initials = r.Sender.Username.Substring(0, 1).ToUpper()
               })
               .ToList();
        }

        public async Task<Result<object>> SendRequestAsync(string senderUserId, string receiverUsername)
        {
            var receiverUserId = _context.Users
                .Where(u => u.Username == receiverUsername)
                .Select(u => u.Id)
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(receiverUserId)) return Result<object>.Fail("Alıcı bulunamadı.");

            var friendRequest = _context.FriendRequests.Any(r => r.SenderUserId == senderUserId && r.Receiver.Username == receiverUsername ||
                                                 r.Sender.Username == receiverUsername && r.ReceiverUserId == senderUserId);
            if (friendRequest)
                return Result<object>.Fail("İşlem gerçekleştirilemedi. Alıcı veya gönderici bilgileri geçersiz.");

            _context.FriendRequests.Add(new FriendRequest
            {
                Id = Guid.NewGuid().ToString(),
                ReceiverUserId = receiverUserId,
                CreatedAt = DateTime.UtcNow,
                SenderUserId = senderUserId ?? string.Empty,
                Status = FriendRequestStatus.Pending
            });
            _context.SaveChanges();

            var senderUsername = _context.Users
                .Where(u => u.Id == senderUserId)
                .Select(u => u.Username)
                .FirstOrDefault();

            var data = new GetPendingRequestsDto
            {
                SenderUsername = senderUsername ?? string.Empty,
                SenderUserId = senderUserId ?? string.Empty,
                Initials = senderUsername?.Substring(0, 1).ToUpper() ?? string.Empty
            };

            // SignalR ile anlık gönderim
            await _hub.Clients.User(receiverUserId).SendAsync(HubEvents.PendingRequestsUpdated, data);
            return Result<object>.Ok();
        }

        public Result<RespondFriendRequestDto> RespondRequest(string receiverUsername, string senderUsername, bool accept)
        {
            var request = _context.FriendRequests
                .FirstOrDefault(r => r.Sender.Username == senderUsername && r.Receiver.Username == receiverUsername);

            if (request == null)            
                return Result<RespondFriendRequestDto>.Fail("Daha önce arkadaş eklediğiniz kullanıcı ile arkadaş olamazsınız.");

            request.Status = accept ? FriendRequestStatus.Accepted : FriendRequestStatus.Rejected;
            _context.SaveChanges();

            if (request.Status == FriendRequestStatus.Accepted)
            {
                var data = new RespondFriendRequestDto
                {
                    Username = senderUsername ?? string.Empty,
                    Initials = senderUsername?.Substring(0, 1).ToUpper() ?? string.Empty,
                    IsOnline = ChatHub.OnlineUsers.Any(o => o.Value.Equals(request.SenderUserId, StringComparison.OrdinalIgnoreCase)),
                    UserId = request.SenderUserId
                };

                var friend = new RespondFriendRequestDto
                {
                    Username = receiverUsername ?? string.Empty,
                    Initials = receiverUsername?.Substring(0, 1).ToUpper() ?? string.Empty,
                    IsOnline = ChatHub.OnlineUsers.Any(o => o.Value.Equals(request.ReceiverUserId, StringComparison.OrdinalIgnoreCase)),
                    UserId = request.ReceiverUserId
                };

                Task.Run(async () =>
                {
                    await _hub.Clients.User(data.UserId).SendAsync(HubEvents.FriendListUpdated, friend);
                });

                return Result<RespondFriendRequestDto>.Ok(data);
            }
            return Result<RespondFriendRequestDto>.Ok();
        }
    }
}
