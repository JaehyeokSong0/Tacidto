using JaehyeokSong0.Tacidto.Application.Chat;
using Unity.Netcode;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JaehyeokSong0.Tacidto.Server
{
    public class ServerSceneScope : LifetimeScope
    {
        [Header("GameObjects to inject")]
        [SerializeField] private ServerManager _serverManager;

        [Header("Prefabs to inject")]
        [SerializeField] private DedicatedChatConnection _dedicatedChatConnection;


        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_serverManager)
                   .AsSelf();

            builder.RegisterComponent(_serverManager.GetComponent<NetworkManager>())
                   .As<NetworkManager>();

            // Chat System
            builder.Register<ChatProcessor>(Lifetime.Singleton)
                   .As<IChatProcessor>();

            builder.RegisterComponentInNewPrefab(_dedicatedChatConnection, Lifetime.Singleton)
                   .As<IChatConnection>();

            builder.Register<ChatSystem>(Lifetime.Singleton)
                   .AsSelf();
        }
    }
}