using Cysharp.Threading.Tasks;
using JaehyeokSong0.Tacidto.Application.Chat;
using JaehyeokSong0.Tacidto.Network;
using VContainer;
using VContainer.Unity;

namespace JaehyeokSong0.Tacidto.Application.Scene
{
    public class LobbySceneManager : SceneManagerBase, IStartable
    {
        private ChatSystem _chatSystem;

        [Inject]
        public LobbySceneManager(ChatSystem chatSystem)
        {
            _chatSystem = chatSystem;
        }



        void IStartable.Start()
        {
            UniTask.SwitchToMainThread();

            _chatSystem.Initialize();
        }
    }
}