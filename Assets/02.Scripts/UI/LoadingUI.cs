using JaehyeokSong0.Tacidto.Application.Version;
using JaehyeokSong0.Tacidto.Network;
using JaehyeokSong0.Tacidto.UI;
using JaehyeokSong0.Tacidto.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace JaehyeokSong0.Tacidto.Application.UI
{
    /// <summary>
    /// LoadingScene에서 Version Controller로부터 진행 정보를 받아와 사용자에게 제공합니다.
    /// </summary>
    public class LoadingUI : UIBase, IVersionView
    {
        [Inject] VersionController _versionController;
        [Inject] ServerConnector _serverConnector;

        [SerializeField] private TMP_Text _progressText;
        [SerializeField] private Slider _progressSlider;

        private void Awake()
        {
            _versionController.Progress
                .Subscribe(value => UpdateProgressUI(value))
                .AddTo(this);
        }

        public void UpdateProgressText(string progressText)
        {
            _progressText.text = progressText;
        }

        public void UpdateProgressUI(float progress)
        {
            _progressSlider.value = progress;    
        }
    }
}