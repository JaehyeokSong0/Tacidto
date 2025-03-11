using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace JaehyeokSong0.Tacidto.UI
{
    /// <summary>
    /// TitleScene의 UI를 관리합니다.
    /// </summary>
    public class TitleSceneUI : UIBase
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        public void Initialize(UnityAction onStart, UnityAction onSettings, UnityAction onExit)
        {
            _startButton.onClick.AddListener(onStart);
            _settingsButton.onClick.AddListener(onSettings);
            _exitButton.onClick.AddListener(onExit);
        }

        private void OnDestroy()
        {
            _startButton.onClick.RemoveAllListeners();
            _settingsButton.onClick.RemoveAllListeners();
            _exitButton.onClick.RemoveAllListeners();
        }
    }
}