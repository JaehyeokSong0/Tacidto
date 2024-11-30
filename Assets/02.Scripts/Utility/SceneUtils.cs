using Cysharp.Threading.Tasks;
using System;

namespace JaehyeokSong0.Tacidto.Utility
{
    public static class SceneUtils
    {
        public enum SceneIndex
        {
            LoadingScene,
            TitleScene,

            // 서버 전용 씬
            ServerScene,
        }

        public static async UniTask LoadScene(SceneIndex sceneIndex)
        {
            string sceneName = sceneIndex.ToString();

            try
            {
                await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneName).ToUniTask();

                DebugUtils.Log($"Scene {sceneName} loaded successfully");
            }
            catch (Exception ex)
            {
                DebugUtils.LogError($"Failed to load scene {sceneName}: {ex.Message}");

                throw;
            }
        }

        public static async UniTask DelayForSeconds(float second)
        {
            await UniTask.SwitchToMainThread();
            await UniTask.Delay(TimeSpan.FromSeconds(second));
        }
    }
}