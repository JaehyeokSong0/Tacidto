using System;
using Unity.Netcode;

namespace JaehyeokSong0.Tacidto.Application.Chat
{
    public enum ChatMessageType
    {
        Normal,
        System
    }

    public enum ChatChannel
    {
        Global, // Dedicated
        Lobby,  // Lobby, Relay (UGS)
        Session // Lobby, Relay (UGS)
    }


    public class ChatMessage : INetworkSerializable
    {
        public string Content { get; set; }
        public string SenderName { get; set; }
        public ChatMessageType MessageType { get; set; }
        public ChatChannel Channel { get; set; }
        public DateTime Timestamp { get; set; }

        //TODO : 내코드가 아님
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // 문자열 직렬화 - FixedString 또는 필요에 따른 다른 방식 사용
            if (serializer.IsReader)
            {
                // Reader 모드에서는 값을 읽어옴
                string tempContent = string.Empty;
                string tempSender = string.Empty;
                serializer.SerializeValue(ref tempContent);
                serializer.SerializeValue(ref tempSender);
                Content = tempContent;
                SenderName = tempSender;
            }
            else
            {
                // Writer 모드에서는 값을 쓰기
                string tempContent = Content ?? string.Empty;
                string tempSender = SenderName ?? string.Empty;
                serializer.SerializeValue(ref tempContent);
                serializer.SerializeValue(ref tempSender);
            }

            // 열거형 값 직렬화
            byte msgType = (byte)MessageType;
            serializer.SerializeValue(ref msgType);
            if (serializer.IsReader)
            {
                MessageType = (ChatMessageType)msgType;
            }

            byte chanType = (byte)Channel;
            serializer.SerializeValue(ref chanType);
            if (serializer.IsReader)
            {
                Channel = (ChatChannel)chanType;
            }

            // DateTime을 Ticks(long)로 직렬화
            long ticks = Timestamp.Ticks;
            serializer.SerializeValue(ref ticks);
            if (serializer.IsReader)
            {
                Timestamp = new DateTime(ticks);
            }
        }
    }
}