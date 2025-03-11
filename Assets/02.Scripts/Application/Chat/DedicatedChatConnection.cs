using JaehyeokSong0.Tacidto.Utility;
using System;
using UniRx;
using Unity.Netcode;
using VContainer;

namespace JaehyeokSong0.Tacidto.Application.Chat
{
    public class DedicatedChatConnection : NetworkBehaviour, IChatConnection
    {
        public IObservable<ChatMessage> OnMessageReceived => _messageReceived;


        private readonly Subject<ChatMessage> _messageReceived = new Subject<ChatMessage>();
        [Inject] private NetworkManager _networkManager;


        #region Event Functions
        public override void OnDestroy()
        {
            base.OnDestroy();
            _messageReceived?.Dispose();
        }
        #endregion

        public bool SendMessage(ChatMessage message)
        {
            try
            {
                if (_networkManager.IsServer == false)
                {
                    DebugUtils.LogError("Only server can send system messages");

                    return false;
                }

                BroadcastSystemMessageClientRpc(message);
                return true;
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"Failed to send message: {ex.Message}");

                return false;
            }
        }

        [ClientRpc]
        private void BroadcastSystemMessageClientRpc(ChatMessage message)
        {
            _messageReceived.OnNext(message);
        }
    }
}