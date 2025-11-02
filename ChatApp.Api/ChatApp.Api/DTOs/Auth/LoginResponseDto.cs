namespace ChatApp.Api.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string UserId { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string Token { get; set; } = null!;
    }
}
