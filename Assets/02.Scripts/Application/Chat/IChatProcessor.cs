namespace JaehyeokSong0.Tacidto.Application.Chat
{
    /// <summary>
    /// 채팅 메시지의 유효성 검사 및 생성 등을 관리합니다.
    /// </summary>
    public interface IChatProcessor
    {
        bool ValidateMessage(ChatMessage message);
        ChatMessage CreateSystemMessage(string content);
        ChatMessage CreateUserMessage(string content, string senderName);
    }
}