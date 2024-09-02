using Cysharp.Threading.Tasks;
using JaehyeokSong0.Tacidto.Environment;
using JaehyeokSong0.Tacidto.Utility;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace JaehyeokSong0.Tacidto.Application.Version
{
    /// <summary>
    /// 게임의 버전을 확인하고 Addressable을 통해 리소스를 최신 상태로 관리합니다.
    /// </summary>
    public class VersionController
    {
        public IReadOnlyReactiveProperty<float> Progress => _progress;
        private ReactiveProperty<float> _progress = new ReactiveProperty<float>();

        public async UniTask StartUpdate()
        {
            try
            {
                Configure();

                var catalog = await CheckCatalogUpdates();

                if (catalog != null && catalog.Count > 0)
                {
                    var updatedCatalog = await UpdateCatalog(catalog);

                    await DownloadDependencies(updatedCatalog);
                }
            }
            catch (Exception ex)
            {
                DebugUtility.LogError($"Exception in VersionController : {ex}");
            }
        }

        private void Configure()
        {
            Addressables.WebRequestOverride = (webRequst) =>
            {
                webRequst.SetRequestHeader("Authorization", "Basic " + AddressableEnvironment.BUCKET_ACCESS_TOKEN);
            };
        }

        private async UniTask<List<string>> CheckCatalogUpdates()
        {
            var updateHandle = Addressables.CheckForCatalogUpdates(false);
            await updateHandle.ToUniTask();

            if (updateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                return updateHandle.Result;
            }
            else
            {
                throw new Exception("Failed to check updates for catalog");
            }
        }

        private async UniTask<List<IResourceLocator>> UpdateCatalog(List<string> catalog)
        {
            var updateHandle = Addressables.UpdateCatalogs(catalog);
            await updateHandle.ToUniTask();

            if (updateHandle.Status == AsyncOperationStatus.Succeeded)
            {
                return updateHandle.Result;
            }
            else
            {
                throw new Exception("Failed to update catalog");
            }
        }

        private async UniTask DownloadDependencies(List<IResourceLocator> catalog)
        {
            var downloadHandle = Addressables.DownloadDependenciesAsync(catalog);
            await downloadHandle.ToUniTask();

            while (_progress.Value < 1f)
            {
                _progress.Value = downloadHandle.PercentComplete;
                await UniTask.Yield();
            }

            if (downloadHandle.Status != AsyncOperationStatus.Succeeded)
            {
                throw new Exception("Failed to download dependencies");
            }
        }
    }
}