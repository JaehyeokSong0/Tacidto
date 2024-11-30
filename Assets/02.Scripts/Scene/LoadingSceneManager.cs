using Cysharp.Threading.Tasks;
using JaehyeokSong0.Tacidto.Application.Version;
using JaehyeokSong0.Tacidto.Network;
using JaehyeokSong0.Tacidto.Utility;
using System;
using UniRx;
using VContainer;
using VContainer.Unity;

namespace JaehyeokSong0.Tacidto.Application.Scene
{
    /// <summary>
    /// Loading scene의 flow를 관리합니다.
    /// LoadingSceneScope에서 Scoped으로 관리됩니다.
    /// </summary>
    public class LoadingSceneManager : SceneManagerBase
    {
        public enum LoadingPhase
        {
            None,
            ResourceUpdate,
            ServerConnection,
            Completed,
            Failed
        }

        public IReadOnlyReactiveProperty<LoadingPhase> CurrentPhase => _currentPhase;
        public ResourceVersionController Version => _versionController;


        [Inject] private LifetimeScope _scope;
        [Inject] private ResourceVersionController _versionController;
        [Inject] private ServerConnector _serverConnector;

        private IVersionView _loadingUI;
        private ReactiveProperty<LoadingPhase> _currentPhase = new ReactiveProperty<LoadingPhase>(LoadingPhase.None);


        private async void Start()
        {
            _loadingUI = _scope.Container.Resolve<IVersionView>();
            _loadingUI.Bind();

            await ProcessLoading();

            await SceneUtils.DelayForSeconds(0.5f);
            _loadingUI.Unbind();
        }

        private async UniTask ProcessLoading()
        {
            try
            {
                _currentPhase.Value = LoadingPhase.ResourceUpdate;
                bool versionControl = await _versionController.StartUpdate();

                if (versionControl == false)
                {
                    await HandleFailure("Version update failed.");

                    return;
                }

                await SceneUtils.DelayForSeconds(0.5f);

                _currentPhase.Value = LoadingPhase.ServerConnection;
                bool serverConnection = await _serverConnector.ConnectToServer();
                await UniTask.SwitchToMainThread();

                if (serverConnection == false)
                {
                    await HandleFailure("Connecting to server failed.");

                    return;
                }

                _currentPhase.Value = LoadingPhase.Completed;

                await SceneUtils.DelayForSeconds(0.5f);
                await SceneUtils.LoadScene(SceneUtils.SceneIndex.TitleScene);
            }
            catch (Exception ex)
            {
                await HandleFailure($"Unexpected error occured : {ex.Message}");
            }
        }

        private async UniTask HandleFailure(string errorMessage, float delaySeconds = 0.5f)
        {
            // 메인 스레드로 전환
            await UniTask.SwitchToMainThread();
            
            DebugUtils.LogError(errorMessage);
            _currentPhase.Value = LoadingPhase.Failed;

            await SceneUtils.DelayForSeconds(delaySeconds);
        }
    }
}