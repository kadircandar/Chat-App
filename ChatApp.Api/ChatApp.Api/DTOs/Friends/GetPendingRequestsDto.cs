namespace ChatApp.Api.DTOs.Friends
{
    public class GetPendingRequestsDto
    {
        public string SenderUsername { get; set; } = string.Empty;
        public string SenderUserId { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
    }
}
