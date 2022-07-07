using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildBundlesWindow : EditorWindow
{
    private string _buildDir = "../Mod/Assets";
    private const string _buttonText = "Build";

    [MenuItem("Window/Build Bundles")]
    private static void Init()
    {
        var window = GetWindow<BuildBundlesWindow>();
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Build Asset Bundle", EditorStyles.boldLabel);
        _buildDir = EditorGUILayout.TextField("Build Directory", _buildDir);

        if (GUILayout.Button(_buttonText))
        {
            if (!Directory.Exists(_buildDir))
            {
                Directory.CreateDirectory(_buildDir);
            }

            BuildPipeline.BuildAssetBundles(_buildDir, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
        }
    }
}
