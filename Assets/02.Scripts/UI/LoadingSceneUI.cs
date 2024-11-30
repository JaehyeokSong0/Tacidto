using JaehyeokSong0.Tacidto.Application.Scene;
using JaehyeokSong0.Tacidto.Application.Version;
using JaehyeokSong0.Tacidto.Network;
using JaehyeokSong0.Tacidto.UI;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;
using static JaehyeokSong0.Tacidto.Application.Scene.LoadingSceneManager;

namespace JaehyeokSong0.Tacidto.Application.UI
{
    /// <summary>
    /// LoadingScene의 진행 정보를 사용자에게 제공합니다.
    /// </summary>
    public class LoadingSceneUI : UIBase, IVersionView
    {
        private const string MESSAGE_DOWNLOADING_RESOURCES = "Downloading resources...";
        private const string MESSAGE_DOWNLOADING_RESOURCES_COMPLETED = "Downloading resources completed";
        private const string MESSAGE_CONNECTING_TO_SERVER = "Connecting to server...";
        private const string MESSAGE_LOADING_COMPLETED = "All loading sequences have been completed successfully";
        private const string MESSAGE_LOADING_FAILED = "Error occured during the loading process";

        [Inject] private LoadingSceneManager _loadingSceneManager;

        private CompositeDisposable _disposables;

        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private Slider _progressSlider;

        public void Bind()
        {
            Unbind(); // 이전 바인딩이 존재한다면 해제
            _disposables = new CompositeDisposable();

            _loadingSceneManager.CurrentPhase
            .Subscribe(phase =>
            {
                _progressText.text = phase switch
                {
                    LoadingPhase.ResourceUpdate => MESSAGE_DOWNLOADING_RESOURCES,
                    LoadingPhase.ServerConnection => MESSAGE_CONNECTING_TO_SERVER,
                    LoadingPhase.Failed => MESSAGE_LOADING_FAILED,
                    LoadingPhase.Completed => MESSAGE_LOADING_COMPLETED,
                    _ => string.Empty
                };

                _progressSlider.value = phase switch
                {
                    LoadingPhase.ServerConnection => 0.5f,
                    LoadingPhase.Completed => 1f,
                    _ => _progressSlider.value
                };
            })
            .AddTo(_disposables);
        }

        public void Unbind()
        {
            _disposables?.Dispose();
            _disposables = null;
        }
    }
}