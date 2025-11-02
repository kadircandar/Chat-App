using ChatApp.Api.DTOs.Friends;
using ChatApp.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FriendsController : ControllerBase
    {

        private readonly IFriendService _friendService;

        public FriendsController(IFriendService friendService)
        {

            _friendService = friendService;
        }

        // GET: api/friends/list
        [HttpGet("list")]
        public List<GetFriendsDto> GetFriends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var friends = _friendService.GetFriends(userId);
            return friends;
        }

        // GET: api/friends/requests/pending
        [HttpGet("requests/pending")]
        public List<GetPendingRequestsDto> GetPendingRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var requests = _friendService.GetPendingRequests(userId);
            return requests;
        }

        // POST: api/friends/requests
        [HttpPost("requests")]
        public async Task<IActionResult> SendFriendRequest([FromBody] SendFriendRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            var result = await _friendService.SendRequestAsync(userId, request.FriendUsername);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok();
        }

        // PATCH: api/friends/requests/{senderUsername}?accept=true/false
        [HttpPatch("requests/{senderUsername}")]
        public ActionResult<RespondFriendRequestDto> RespondFriendRequest(string senderUsername, [FromQuery] bool accept)
        {
            var username = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var result = _friendService.RespondRequest(username, senderUsername, accept);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Data);
        }
    }
}
