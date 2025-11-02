namespace ChatApp.Api.Models
{
    public class FriendRequest
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SenderUserId { get; set; } = null!;
        public User Sender { get; set; } = null!;
        public string ReceiverUserId { get; set; } = null!;
        public User Receiver { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public FriendRequestStatus Status { get; set; } = FriendRequestStatus.Pending;
    }

    public enum FriendRequestStatus
    {
        Pending,
        Accepted,
        Rejected
    }
}
