namespace ChatApp.Api.DTOs.Friends
{
    public class GetFriendsDto
    {
        public string UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public bool IsOnline { get; set; } = false; // online durum için
        public int UnreadCount { get; set; } = 0;
        public string Initials { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
        public DateTime? Time { get; set; }
        public string SenderUserId { get; set; }
    }
}
