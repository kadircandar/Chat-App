using ChatApp.Api.Data;
using ChatApp.Api.DTOs;
using ChatApp.Api.DTOs.Messages;
using ChatApp.Api.Hubs;
using ChatApp.Api.Models;
using ChatApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.Api.Services.Implementations
{
    public class MessageService : IMessageService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _hub;

        public MessageService(AppDbContext context, IHubContext<ChatHub> hub)
        {
            _context = context;
            _hub = hub;
        }

        public async Task<Result<object>> SendMessage(SendMessageDto dto)
        {
            var hasSender = await _context.Users.AnyAsync(u => u.Id == dto.SenderUserId);
            var hasReceiver = await _context.Users.AnyAsync(u => u.Id == dto.ReceiverUserId);

            if (!hasSender || !hasReceiver) return Result<object>.Fail("Gönderici veya Alıcı bulunamadı");

            var message = new Message
            {
                SenderUserId = dto.SenderUserId,
                ReceiverUserId = dto.ReceiverUserId,
                ContentEncrypted = dto.Content,
                SentAt = DateTime.UtcNow,
                IsRead = dto.IsRead
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            var receiveMessage = new ReceiveMessageDto
            {
                Content = dto.Content,
                IsRead = dto.IsRead,
                ReceiverUserId = dto.ReceiverUserId,
                SenderUserId = dto.SenderUserId,
                SentAt = message.SentAt
            };

            // SignalR ile anlık gönderim
            await _hub.Clients.User(dto.ReceiverUserId).SendAsync(HubEvents.ReceiveMessage, receiveMessage);

            return Result<object>.Ok();
        }

        public async Task<Result<object>> DeleteMessage(string userId, string messageId)
        {
            var message = await _context.Messages.AnyAsync(m => m.Id == messageId);
            if (!message) Result<object>.Fail("Mesaj bulunamadı.");

            var result = await _context.Messages
                 .Where(m => m.Id == messageId && m.SenderUserId == userId)
                 .ExecuteDeleteAsync();

            return Result<object>.Ok();
        }

        public async Task<Result<List<GetMessageHistoryDto>>> GetMessageHistory(string userId, string friendUserId)
        {
            var otherUser = await _context.Users.AnyAsync(u => u.Id == friendUserId);

            if (userId == null || !otherUser) Result<object>.Fail("Gönderici veya Alıcı bulunamadı.");

            var messages = await _context.Messages
                .Where(m => (m.SenderUserId == userId && m.ReceiverUserId == friendUserId) ||
                            (m.SenderUserId == friendUserId && m.ReceiverUserId == userId))
                .OrderBy(m => m.SentAt)
                .Select(m => new GetMessageHistoryDto
                {
                    Id = m.Id,
                    SenderUserId = m.SenderUserId,
                    ReceiverUserId = m.ReceiverUserId,
                    Content = m.ContentEncrypted,
                    SentAt = m.SentAt,
                    IsRead = m.IsRead
                })
                .ToListAsync();

            return Result<List<GetMessageHistoryDto>>.Ok(messages);
        }

        public async Task<Result<object>> MarkAsRead(string userId, MarkAsReadDto dto)
        {
            var updatedCount = await _context.Messages
                .Where(m => !m.IsRead && m.ReceiverUserId == userId && m.SenderUserId == dto.SenderUserId)
                .ExecuteUpdateAsync(setters => setters
                .SetProperty(m => m.IsRead, true));

            if (updatedCount == 0)
                return Result<object>.Fail("Mesaj bulunamadı.");

            return Result<object>.Ok();
        }
    }
}
