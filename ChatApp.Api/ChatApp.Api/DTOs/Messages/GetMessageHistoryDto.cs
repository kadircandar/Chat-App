namespace ChatApp.Api.DTOs.Messages
{
    public class GetMessageHistoryDto
    {
        public string Id { get; set; } = null!;
        public string SenderUserId { get; set; } = null!;
        public string ReceiverUserId { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime SentAt{ get; set; }
        public bool IsRead { get; set; }
    }
}
