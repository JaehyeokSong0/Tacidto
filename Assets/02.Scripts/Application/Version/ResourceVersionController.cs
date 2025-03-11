using Cysharp.Threading.Tasks;
using JaehyeokSong0.Tacidto.Environment;
using JaehyeokSong0.Tacidto.Utility;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace JaehyeokSong0.Tacidto.Application.Version
{
    /// <summary>
    /// 게임의 버전을 확인하고 Addressable을 통해 리소스를 최신 상태로 관리합니다.
    /// ApplicationScope에서 Singleton으로 관리됩니다.
    /// </summary>
    public class ResourceVersionController
    {
        public enum VersionStatus
        {
            None,
            DownloadingResources,
            DownloadingCompleted,
            DownloadingFailed
        }

        public IReadOnlyReactiveProperty<float> Progress => _progress;
        public IReadOnlyReactiveProperty<VersionStatus> Status => _status;


        private ReactiveProperty<float> _progress = new ReactiveProperty<float>();
        private ReactiveProperty<VersionStatus> _status = new ReactiveProperty<VersionStatus>(VersionStatus.None);


        public async UniTask<bool> StartUpdateAsync()
        {
            try
            {
                _progress.Value = 0f;
                _status.Value = VersionStatus.DownloadingResources;

                Configure();

                var catalog = await CheckCatalogUpdatesAsync();

                // 업데이트할 catalog가 있다면
                if (catalog != null && catalog.Count > 0)
                {
                    var updatedCatalog = await UpdateCatalogAsync(catalog);

                    if (updatedCatalog != null)
                    {
                        await DownloadDependenciesAsync(updatedCatalog);
                    }
                }

                _progress.Value = 1f;
                _status.Value = VersionStatus.DownloadingCompleted;

                return true;
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"Exception in VersionController/StartUpdate : {ex}");

                return false;
            }
        }

        private void Configure()
        {
            Addressables.WebRequestOverride = (webRequst) =>
            {
                webRequst.SetRequestHeader("Authorization", "Basic " + AddressableEnvironment.BUCKET_ACCESS_TOKEN);
            };
        }

        /// <summary>
        /// 새로운 업데이트가 있는지 확인
        /// </summary>
        /// <returns>업데이트가 가능한 catalog ID의 list</returns>
        private async UniTask<List<string>> CheckCatalogUpdatesAsync()
        {
            var updateHandle = Addressables.CheckForCatalogUpdates();

            try
            {
                // [MEMO] await updateHandle.ToUniTask() 할 시 Attempting to use an invalid operation handle 에러 발생 - 원인 불명
                await updateHandle.Task; 
                
                switch (updateHandle.Status)
                {
                    case AsyncOperationStatus.Succeeded:
                        {
                            return updateHandle.Result;
                        }
                    default:
                        {
                            _status.Value = VersionStatus.DownloadingFailed;

                            throw new Exception($"Failed to check updates for catalog: {updateHandle.OperationException?.Message}");
                        }
                }
            }
            finally
            {
                if (updateHandle.IsValid() == true)
                {
                    ReleaseHandle(updateHandle);
                }
            }
        }

        private async UniTask<List<IResourceLocator>> UpdateCatalogAsync(List<string> catalog)
        {
            var updateHandle = Addressables.UpdateCatalogs(catalog);

            try
            {
                await updateHandle.Task;

                if (updateHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    return updateHandle.Result;
                }
                else
                {
                    throw new Exception($"Failed to update catalog: {updateHandle.OperationException?.Message}");
                }
            }
            finally
            {
                if (updateHandle.IsValid() == true)
                {
                    ReleaseHandle(updateHandle);
                }
            }
        }

        private async UniTask DownloadDependenciesAsync(List<IResourceLocator> catalog)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(catalog);

            try
            {
                while (downloadHandle.IsDone == false)
                {
                    _progress.Value = downloadHandle.PercentComplete;
                    await UniTask.Yield();
                }

                if (downloadHandle.Status == AsyncOperationStatus.Failed)
                {
                    _status.Value = VersionStatus.DownloadingFailed;
                    throw new Exception($"Failed to download dependencies: {downloadHandle.OperationException?.Message}");
                }

                _status.Value = VersionStatus.DownloadingCompleted;
                _progress.Value = 1f;
            }
            finally
            {
                ReleaseHandle(downloadHandle);
            }
        }

        private void ReleaseHandle(AsyncOperationHandle handle)
        {
            if (handle.IsValid() == true)
            {
                try
                {
                    Addressables.Release(handle);
                }
                catch (Exception ex)
                {
                    DebugUtils.LogError($"Error releasing handle: {ex}");
                }
            }
        }
    }
}