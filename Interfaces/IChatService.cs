public interface IChatService
{
    List<ChatDto> GetChats(int userId);
    bool SendMessage(SendMessageDto messageDto, int senderId);
}
