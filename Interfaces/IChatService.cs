public interface IChatService
{
    List<ChatDto> GetChats(string userId);
    bool SendMessage(SendMessageDto messageDto, string senderId);
}
