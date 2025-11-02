namespace ChatApp.Api.DTOs.Messages
{
    public class ReceiveMessageDto : SendMessageDto
    {
        public DateTime SentAt { get; set; }
    }
}
