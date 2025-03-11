using Cysharp.Threading.Tasks;
using JaehyeokSong0.Tacidto.Utility;
using System;
using UniRx;
using VContainer;

namespace JaehyeokSong0.Tacidto.Application.Chat
{
    public class ChatSystem
    {
        public IObservable<ChatMessage> OnMessageReceived => _chatConnection.OnMessageReceived;

        [Inject] private readonly IChatProcessor _chatProcessor;
        [Inject] private readonly IChatConnection _chatConnection;

        private CompositeDisposable _disposables;
        private bool _isInitialized = false;


        public void Initialize()
        {
            if (_isInitialized == true)
                return;

            _disposables = new CompositeDisposable();

            _chatConnection.OnMessageReceived?
                           .Subscribe(HandleMessageReceived)
                           .AddTo(_disposables);

            _isInitialized = true;
        }

        public void ShutDown()
        {
            if (_isInitialized == false)
            {
                return;
            }

            _disposables?.Clear();
            _disposables = null;
            _isInitialized = false;

            DebugUtils.Log("Chat system shutdown");
        }

        public bool SendMessage(string content, string senderName, ChatChannel channel)
        {
            if (_isInitialized == false)
            {
                DebugUtils.LogError("Cannot send message: Chat system is not initialized");
                return false;
            }

            if (string.IsNullOrEmpty(content) == true)
            {
                return false;
            }

            var message = _chatProcessor.CreateUserMessage(content, senderName);
            message.Channel = channel;

            if (_chatProcessor.ValidateMessage(message) == false)
            {
                return false;
            }

            return _chatConnection.SendMessage(message);
        }

        public bool SendSystemMessage(string content)
        {
            if (_isInitialized == false)
            {
                DebugUtils.LogError("Cannot send system message: Chat system is not initialized");
                return false;
            }

            if (string.IsNullOrEmpty(content) == true)
            {
                return false;
            }

            var message = _chatProcessor.CreateSystemMessage(content);

            if (_chatProcessor.ValidateMessage(message) == false)
            {
                return false;
            }

            return _chatConnection.SendMessage(message);
        }


        private void HandleMessageReceived(ChatMessage message)
        {
            DebugUtils.Log($"[Message] ({message.Timestamp}) \t {message.SenderName} : {message.Content}");
        }
    }
}