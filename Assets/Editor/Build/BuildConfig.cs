using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildConfig", menuName = "Build/BuildConfig")]
public class BuildConfig : ScriptableObject
{
    public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    public List<SceneAsset> scenes = new List<SceneAsset>();
    public bool isServerBuild = false;
}