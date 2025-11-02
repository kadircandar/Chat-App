namespace ChatApp.Api.Models
{
    public class Message
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SenderUserId { get; set; } = null!;
        public User Sender { get; set; } = null!;
        public string ReceiverUserId { get; set; } = null!;
        public User Receiver { get; set; } = null!;
        public string ContentEncrypted { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
        public bool IsRead { get; set; } = false;
    }
}
