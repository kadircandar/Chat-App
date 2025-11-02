using ChatApp.Api.DTOs;
using ChatApp.Api.DTOs.Messages;
using Microsoft.AspNetCore.Mvc;

namespace ChatApp.Api.Services.Interfaces
{
    public interface IMessageService
    {
        Task<Result<object>> SendMessage(SendMessageDto dto);
        Task<Result<List<GetMessageHistoryDto>>> GetMessageHistory(string userId, string friendUserId);
        Task<Result<object>> DeleteMessage(string userId, string messageId);
        Task<Result<object>> MarkAsRead(string userId, MarkAsReadDto dto);
    }
}
