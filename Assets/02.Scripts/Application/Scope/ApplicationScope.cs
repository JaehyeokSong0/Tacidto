using JaehyeokSong0.Tacidto.Application.Version;
using JaehyeokSong0.Tacidto.Network;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JaehyeokSong0.Tacidto.Application.Scope
{
    /// <summary>
    /// 게임의 전반적인 생명 주기에 대한 scope를 관리합니다.
    /// </summary>
    public class ApplicationScope : LifetimeScope
    {
        [Header("Prefabs to inject")]
        [SerializeField] private ServerConnector _serverConnector;

        protected override void Awake()
        {
            base.Awake();

            DontDestroyOnLoad(gameObject);
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.Register<VersionController>(Lifetime.Singleton);
            builder.RegisterComponentInNewPrefab(_serverConnector, Lifetime.Singleton);
        }
    }
}