namespace ChatApp.Api.Models
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Username { get; set; } = null!;
        public string PasswordHash { get; set; } = null!; 
        public string? DisplayName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  
        public ICollection<Message> SentMessages { get; set; } = new List<Message>();
        public ICollection<Message> ReceivedMessages { get; set; } = new List<Message>();
        public ICollection<FriendRequest> FriendRequestsSent { get; set; } = new List<FriendRequest>();
        public ICollection<FriendRequest> FriendRequestsReceived { get; set; } = new List<FriendRequest>();
    }
}
