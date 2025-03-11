using JaehyeokSong0.Tacidto.Utility;
using System;
using VContainer.Unity;

namespace JaehyeokSong0.Tacidto.Application.Scene
{
    /// <summary>
    /// 각 scene별 전반적인 game flow를 관리합니다.
    /// Container 내에서 EntryPoint로 지정되어야 합니다.
    /// </summary>
    public abstract class SceneManagerBase : IInitializable, IDisposable
    {
        protected bool _isInitialized = false;

        // IInitializable.Initialize는 Monobehaviour.OnEnable 이후에 실행됨.
        void IInitializable.Initialize()
        {
            if (_isInitialized == true)
            {
                return;
            }

            DebugUtils.Log($"Initializing scene [{this}]...");

            OnInitialize();
            _isInitialized = true;
        }

        void IDisposable.Dispose()
        {
            if (_isInitialized == false)
            {
                return;
            }

            DebugUtils.Log($"Disposing scene [{this}]...");

            OnDispose();
            _isInitialized = false;
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnDispose()
        {
        }
    }
}