using JaehyeokSong0.Tacidto.Application.Version;
using JaehyeokSong0.Tacidto.Network;
using UnityEngine;
using VContainer;

namespace JaehyeokSong0.Tacidto.Application
{
    /// <summary>
    /// 게임의 전체적인 상태나 flow를 관리합니다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; } = null;

        [Inject] private VersionController _versionController;
        [Inject] private ServerConnector _serverConnector;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
        }

        private async void Start()
        {
            await _versionController.StartUpdate();
            await _serverConnector.ConnectToServer();
        }
    }
}