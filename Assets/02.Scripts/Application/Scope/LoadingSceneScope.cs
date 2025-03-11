using JaehyeokSong0.Tacidto.Application.Scene;
using JaehyeokSong0.Tacidto.Application.UI;
using JaehyeokSong0.Tacidto.Application.Version;
using JaehyeokSong0.Tacidto.Utility;
using System;
using UniRx;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using static JaehyeokSong0.Tacidto.Application.Scene.LoadingSceneManager;

namespace JaehyeokSong0.Tacidto.Application.Scope
{
    public class LoadingSceneScope : LifetimeScope
    {
        [Header("GameObjects to inject")]
        [SerializeField] private LoadingSceneUI _loadingUI;


        private ReactiveProperty<LoadingPhase> _loadingPhase;


        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterEntryPointExceptionHandler(ex =>
            {
                DebugUtils.LogError($"Error in LoadingSceneManager : {ex.Message}");
            });

            builder.RegisterEntryPoint<LoadingSceneManager>(Lifetime.Scoped)
                   .AsSelf();

            _loadingPhase = new ReactiveProperty<LoadingPhase>(LoadingPhase.None);

            builder.RegisterInstance(_loadingPhase)
                   .As<ReactiveProperty<LoadingPhase>>()
                   .As<IReadOnlyReactiveProperty<LoadingPhase>>();

            builder.RegisterComponent(_loadingUI)
                   .As<IVersionView>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _loadingPhase?.Dispose();
        }
    }
}