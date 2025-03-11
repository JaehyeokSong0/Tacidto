using Cysharp.Threading.Tasks;
using JaehyeokSong0.Tacidto.Application.Version;
using JaehyeokSong0.Tacidto.Network;
using JaehyeokSong0.Tacidto.Utility;
using System;
using System.Threading;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace JaehyeokSong0.Tacidto.Application.Scene
{
    /// <summary>
    /// Loading scene의 flow를 관리합니다.
    /// LoadingSceneScope에서 Scoped으로 관리됩니다.
    /// </summary>
    public class LoadingSceneManager : SceneManagerBase, IAsyncStartable
    {
        public enum LoadingPhase
        {
            None,
            ResourceUpdate,
            ServerConnection,
            Completed,
            Failed
        }


        private ResourceVersionController _versionController;
        private ServerConnector _serverConnector;
        private IVersionView _loadingUI;
        private ReactiveProperty<LoadingPhase> _currentPhase;


        [Inject]
        public LoadingSceneManager
            (
                ResourceVersionController versionController,
                ServerConnector serverConnector,
                IVersionView loadingUI,
                ReactiveProperty<LoadingPhase> loadingPhase
            )
        {
            _versionController = versionController;
            _serverConnector = serverConnector;
            _loadingUI = loadingUI;
            _currentPhase = loadingPhase;
        }


        #region Event Functions
        public async UniTask StartAsync(CancellationToken cancellation = default)
        {
            DebugUtils.Log($"StartAsync Called : {this.GetHashCode()}");
            _loadingUI.Bind();

            await ProcessLoadingAsync();

            await SceneUtils.DelayForSecondsAsync(0.5f);
            _loadingUI.Unbind();
        }
        #endregion


        private async UniTask ProcessLoadingAsync()
        {
            try
            {
                _currentPhase.Value = LoadingPhase.ResourceUpdate;
                bool versionControl = await _versionController.StartUpdateAsync();

                if (versionControl == false)
                {
                    await HandleFailureAsync("Version update failed.");

                    return;
                }

                await SceneUtils.DelayForSecondsAsync(0.5f);

                _currentPhase.Value = LoadingPhase.ServerConnection;
                bool serverConnection = await _serverConnector.ConnectToServerAsync();
                await UniTask.SwitchToMainThread();

                if (serverConnection == false)
                {
                    await HandleFailureAsync("Connecting to server failed.");

                    return;
                }

                _currentPhase.Value = LoadingPhase.Completed;

                await SceneUtils.DelayForSecondsAsync(0.5f);
                await SceneUtils.LoadSceneAsync(SceneUtils.SceneIndex.TitleScene);
            }
            catch (Exception ex)
            {
                await HandleFailureAsync($"Unexpected error occured : {ex.Message}");
            }
        }

        private async UniTask HandleFailureAsync(string errorMessage, float delaySeconds = 0.5f)
        {
            // 메인 스레드로 전환
            await UniTask.SwitchToMainThread();

            DebugUtils.LogError(errorMessage);
            _currentPhase.Value = LoadingPhase.Failed;

            await SceneUtils.DelayForSecondsAsync(delaySeconds);
        }
    }
}