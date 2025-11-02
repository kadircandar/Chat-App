namespace ChatApp.Api.DTOs.Messages
{
    public class SendMessageDto
    {
        public string SenderUserId { get; set; } = null!;
        public string ReceiverUserId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public bool IsRead { get; set; } = false;
    }
}
