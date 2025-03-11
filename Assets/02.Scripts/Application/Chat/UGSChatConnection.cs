using JaehyeokSong0.Tacidto.Utility;
using System;
using UniRx;
using VContainer;

namespace JaehyeokSong0.Tacidto.Application.Chat
{
    public class UGSChatConnection : IChatConnection
    {
        public IObservable<ChatMessage> OnMessageReceived => throw new NotImplementedException();

        public bool SendMessage(ChatMessage message)
        {
            try
            {
                // TODO : UGS를 통한 메시지 전송 구현
                return true;
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"Failed to send UGS message: {ex.Message}");
                return false;
            }
        }
    }
}