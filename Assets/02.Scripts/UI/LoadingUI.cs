using JaehyeokSong0.Tacidto.Application.Version;
using JaehyeokSong0.Tacidto.UI;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace JaehyeokSong0.Tacidto.Application.UI
{
    /// <summary>
    /// ResourceLoadingScene에서 Version Controller로부터
    /// </summary>
    public class LoadingUI : UIBase, IVersionView
    {
        [Inject] VersionController _controller;
        [SerializeField] private Slider _progressSlider;

        private void Awake()
        {
            _controller.Progress
                .Subscribe(value => UpdateProgressUI(value))
                .AddTo(this);
        }

        public void UpdateProgressUI(float progress)
        {
            _progressSlider.value = progress;    
        }
    }
}