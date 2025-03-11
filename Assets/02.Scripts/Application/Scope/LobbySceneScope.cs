using JaehyeokSong0.Tacidto.Application.Chat;
using JaehyeokSong0.Tacidto.Application.Scene;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JaehyeokSong0.Tacidto.Application.Scope
{
    public class LobbySceneScope : LifetimeScope
    {
        [Header("GameObjects to inject")]
        [SerializeField] private LobbySceneManager _sceneManager;

        [Header("Prefabs to inject")]
        [SerializeField] private DedicatedChatConnection _dedicatedChatConnection; 

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);

            builder.RegisterComponent(_sceneManager)
                   .AsImplementedInterfaces();

            builder.RegisterComponentInNewPrefab(_dedicatedChatConnection, Lifetime.Singleton)
                   .As<IChatConnection>();

            builder.Register<ChatProcessor>(Lifetime.Singleton)
                   .As<IChatProcessor>();

            builder.Register<ChatSystem>(Lifetime.Singleton)
                   .AsSelf();
        }
    }
}