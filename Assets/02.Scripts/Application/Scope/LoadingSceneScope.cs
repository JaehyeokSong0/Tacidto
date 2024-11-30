using Cysharp.Threading.Tasks;
using JaehyeokSong0.Tacidto.Application.Scene;
using JaehyeokSong0.Tacidto.Application.UI;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JaehyeokSong0.Tacidto.Application.Scope
{
    public class LoadingSceneScope : LifetimeScope
    {
        [Header("GameObjects to inject")]
        [SerializeField] private LoadingSceneManager _sceneManager;
        [SerializeField] private LoadingSceneUI _loadingUI;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(_sceneManager);
            builder.RegisterComponent(_loadingUI)
                   .AsImplementedInterfaces()
                   .AsSelf();
        }

    }
}