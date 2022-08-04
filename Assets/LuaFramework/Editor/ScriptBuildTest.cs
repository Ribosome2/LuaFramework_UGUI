using UnityEditor;
using UnityEngine;

public class ScriptBuildTest : EditorWindow
{
    [MenuItem("Tools/打包编译C#测试窗口")]
    public static void OpenBuildWindow()
    {
        GetWindow<ScriptBuildTest>().titleContent = new GUIContent("C#编译测试");
    }

    private BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    private bool mIsDevelopmentBuild;
    private bool scriptOnly;

    void OnGUI()
    {
        buildTarget = (BuildTarget) EditorGUILayout.EnumPopup(buildTarget);
        mIsDevelopmentBuild = EditorGUILayout.Toggle("Development build", mIsDevelopmentBuild);
        scriptOnly = EditorGUILayout.Toggle("ScriptOnly", scriptOnly);
        GUILayout.Space(10);
        if (GUILayout.Button("测试"))
        {
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();

            buildPlayerOptions.scenes = new[] {"Assets/JustForTest.unity"};
            buildPlayerOptions.locationPathName = "scriptBuildsPathWhatEver";
            buildPlayerOptions.target = buildTarget;
            if (mIsDevelopmentBuild)
            {
                buildPlayerOptions.options = buildPlayerOptions.options | BuildOptions.Development;
            }

            if (scriptOnly)
            {
                buildPlayerOptions.options = buildPlayerOptions.options | BuildOptions.BuildScriptsOnly;
            }


            BuildPipeline.BuildPlayer(buildPlayerOptions);
            Debug.Log("Build完成");
        }
    }
}