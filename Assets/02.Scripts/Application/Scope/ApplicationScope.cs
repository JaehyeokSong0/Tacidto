using JaehyeokSong0.Tacidto.Application.Version;
using JaehyeokSong0.Tacidto.Network;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JaehyeokSong0.Tacidto.Application.Scope
{
    /// <summary>
    /// 게임의 전반적인 생명 주기에 대한 scope를 관리합니다.
    /// Client side에서 사용합니다.
    /// </summary>
    public class ApplicationScope : LifetimeScope
    {
        private static ApplicationScope _instance;

        [Header("Prefabs to inject")]
        [SerializeField] private ServerConnector _serverConnector;

        protected override void Awake()
        {
            if(_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            base.Awake();

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.Register<ResourceVersionController>(Lifetime.Singleton);

            builder.RegisterComponentInNewPrefab(_serverConnector, Lifetime.Singleton);

            builder.RegisterComponent(_serverConnector.GetComponent<NetworkManager>())
                   .AsSelf();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if(_instance == this)
            {
                _instance = null;
            }
        }
    }
}