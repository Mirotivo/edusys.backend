public interface IChatService
{
    List<ChatDto> GetChats(string userId);
    bool SendMessage(SendMessageDto messageDto, string senderId);
    Chat EnsureChatExists(string studentId, string tutorId, int listingId);
}
