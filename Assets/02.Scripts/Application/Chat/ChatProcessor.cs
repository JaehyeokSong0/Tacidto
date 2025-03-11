using System;

namespace JaehyeokSong0.Tacidto.Application.Chat
{
    public class ChatProcessor : IChatProcessor
    {
        public ChatMessage CreateSystemMessage(string content)
        {
            return new ChatMessage
            {
                Content = content,
                SenderName = "System",
                MessageType = ChatMessageType.System,
                Channel = ChatChannel.Global,
                Timestamp = DateTime.Now
            };
        }

        public ChatMessage CreateUserMessage(string content, string senderName)
        {
            return new ChatMessage
            {
                Content = content,
                SenderName = senderName,
                MessageType = ChatMessageType.Normal,
                Channel = ChatChannel.Global, // 호출 시점에 변경 가능
                Timestamp = DateTime.Now
            };
        }

        public bool ValidateMessage(ChatMessage message)
        {
            if (string.IsNullOrEmpty(message.Content) == true)
            {
                return false;
            }

            return true;
        }
    }
}