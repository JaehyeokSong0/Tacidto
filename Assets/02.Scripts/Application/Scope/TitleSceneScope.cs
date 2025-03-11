using JaehyeokSong0.Tacidto.Application.Scene;
using JaehyeokSong0.Tacidto.UI;
using JaehyeokSong0.Tacidto.Utility;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JaehyeokSong0.Tacidto.Application.Scope
{
    public class TitleSceneScope : LifetimeScope
    {
        [Header("GameObjects to inject")]
        [SerializeField] private TitleSceneUI _titleUI;

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterEntryPointExceptionHandler(ex =>
            {
                DebugUtils.LogError($"Error in TitleSceneManager : {ex.Message}");
            });

            builder.RegisterEntryPoint<TitleSceneManager>(Lifetime.Scoped)
                   .AsSelf();

            builder.RegisterComponent(_titleUI)
                   .AsSelf();
        }
    }
}