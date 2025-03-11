using Cysharp.Threading.Tasks;
using System;

namespace JaehyeokSong0.Tacidto.Application.Chat
{
    /// <summary>
    /// 채팅 메시지의 네트워크 통신을 처리합니다.
    /// </summary>
    public interface IChatConnection
    {
        IObservable<ChatMessage> OnMessageReceived { get; }
        bool SendMessage(ChatMessage message);
    }
}