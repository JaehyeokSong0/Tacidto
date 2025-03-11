namespace JaehyeokSong0.Tacidto.Utility
{
    public static class ApplicationUtils
    {
        public static void ExitApplication()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            UnityEngine.Application.Quit();
#endif
        }
    }
}