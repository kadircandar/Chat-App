using ChatApp.Api.DTOs;
using ChatApp.Api.DTOs.Friends;

namespace ChatApp.Api.Services.Interfaces
{
    public interface IFriendService
    {
        List<GetFriendsDto> GetFriends(string userId);
        List<GetPendingRequestsDto> GetPendingRequests(string userId);
        Task<Result<object>> SendRequestAsync(string senderUsername, string receiverUsername);
        Result<RespondFriendRequestDto> RespondRequest(string receiverUsername, string senderUsername, bool accept);
    }
}
