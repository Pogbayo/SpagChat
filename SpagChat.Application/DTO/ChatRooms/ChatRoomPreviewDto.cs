namespace SpagChat.Application.DTO.ChatRooms
{
    public class ChatRoomPreviewDto
    {
        public required string ChatRoomId { get; set; }
        public required string Name { get; set; }
        public bool IsGroup { get; set; }
        public required string PreviewUserName { get; set; }  
        public string? DpUrl { get; set; }
        public required string LastMessage { get; set; }
        public DateTime? LastMessageTimestamp { get; set; }
    }
}
