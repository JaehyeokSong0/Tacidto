using JaehyeokSong0.Tacidto.Utility;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class BuildEditor
{
    private const string BUILD_CONFIG_PATH = "BuildConfigs";
    private static BuildConfig[] configs;

    [MenuItem("CustomEditor/Build/Windows/Build Server and Client %t")]
    public static void BuildAll_Windows()
    {
        // 메소드 내에서 지역변수로 생성하면 캐싱된 configs 리소스가 해제되는 일이 발생하므로 클래스 변수로 생성
        configs = Resources.LoadAll<BuildConfig>(BUILD_CONFIG_PATH);

        foreach (BuildConfig config in configs)
        {
            DebugUtility.Log(config.name);
        }

        foreach (BuildConfig config in configs)
        {
            if (config == null)
            {
                continue;
            }

            DebugUtility.Log($"Found BuildConfig [{config.name}]");

            if ((config.buildTarget == BuildTarget.StandaloneWindows) ||
               ((config.buildTarget == BuildTarget.StandaloneWindows64)))
            {
                DebugUtility.Log($"Try Build [{config.name}]...");
                Build(config);
            }
        }
    }

    private static void Build(BuildConfig config)
    {
        if (config == null)
        {
            DebugUtility.Log($"BuildConfig can not be null");
            return;
        }
        if (config.scenes.Count == 0)
        {
            DebugUtility.LogError($"No scenes found in {config.name}");
            return;
        }
        DebugUtility.Log($"Start building with configuration [{config}]... ");
        SetBuildSettings(config);

        string[] scenePaths = config.scenes
            .Select(path => AssetDatabase.GetAssetPath(path))
            .ToArray();
        string buildPath = GetBuildPath(config) + GetExecutableExtension(config.buildTarget);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = buildPath,
            target = config.buildTarget,
        };

        DebugUtility.Log($"Starting build for {config.name}...");
        DebugUtility.Log($"Build target : {config.buildTarget}");
        DebugUtility.Log($"Output path : {buildPath}");
        DebugUtility.Log($"Scene count : {config.scenes.Count}");

        BuildPipeline.BuildPlayer(options);
    }

    private static string GetBuildPath(BuildConfig config)
    {
        string targetFolderPath;

        if (config.isServerBuild == true)
            targetFolderPath = $"Build/Server/{config.buildTarget}";
        else
            targetFolderPath = $"Build/Client/{config.buildTarget}";

        // Clean build (The target path already exists as a directory 오류 방지용)
        if (Directory.Exists(targetFolderPath) == true)
        {
            Directory.Delete(targetFolderPath, true);
        }

        Directory.CreateDirectory(targetFolderPath);

        return targetFolderPath;
    }
    private static string GetExecutableExtension(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return ".exe";
            case BuildTarget.StandaloneLinux64:
                return ".x86_64";
            default:
                return "";
        }
    }

    /// <summary>
    /// BuildConfig에 따라 에디터 사이드에서 필요한 세팅을 수행합니다.
    /// </summary>
    /// <param name="config"></param>
    private static void SetBuildSettings(BuildConfig config)
    {
        if (config.isServerBuild == true)
        {
            switch (config.buildTarget)
            {
                case BuildTarget.StandaloneLinux64:
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Server;
                    DebugUtility.Log($"Setting build subtarget to Server for {config.buildTarget}");
                    break;
                default:
                    DebugUtility.LogError($"Server build is not supported for {config.buildTarget}. Building as regular player.");
                    EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
                    break;
            }
        }
        else
        {
            EditorUserBuildSettings.standaloneBuildSubtarget = StandaloneBuildSubtarget.Player;
            DebugUtility.Log($"Setting build subtarget to Player for {config.buildTarget}");
        }
    }
}