using JaehyeokSong0.Tacidto.Application.Version;
using JaehyeokSong0.Tacidto.UI;
using JaehyeokSong0.Tacidto.Utility;
using VContainer;
using VContainer.Unity;

namespace JaehyeokSong0.Tacidto.Application.Scene
{
    /// <summary>
    /// Title scene의 flow를 관리합니다.
    /// TitleSceneScope에서 Scoped로 관리됩니다.
    /// </summary>
    public class TitleSceneManager : SceneManagerBase
    {
        private TitleSceneUI _titleUI;

        [Inject]
        public TitleSceneManager(TitleSceneUI titleUI)
        {
            _titleUI = titleUI;
        }

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _titleUI.Initialize(StartGameAsync, OpenSettings, ExitGame);
        }

        public async void StartGameAsync()
        {
            await SceneUtils.LoadSceneAsync(SceneUtils.SceneIndex.LobbyScene);
        }

        public void OpenSettings()
        {
            DebugUtils.Log("Settings");
        }

        public void ExitGame()
        {
            ApplicationUtils.ExitApplication();
        }
    }
}