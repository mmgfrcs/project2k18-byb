using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

[InitializeOnLoad]
public class BuildEditor : EditorWindow, IPreprocessBuildWithReport {
    bool developmentVersion = true, increment = true;
    static bool playModeIncrement = true;

    public BuildEditor()
    {
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
    }

    private void EditorApplication_playModeStateChanged(PlayModeStateChange obj)
    {
        Debug.Log("(BuildEditor) " + playModeIncrement);
        if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
        {
            int[] ver = ParseVersion();
            if (playModeIncrement)
            {
                ver[3]++;
                playModeIncrement = false;
            }
            PlayerSettings.bundleVersion = CollapseVersion(ver, developmentVersion);
        }
        else if (obj == PlayModeStateChange.ExitingPlayMode && playModeIncrement == false)
        {
            playModeIncrement = true;
        }
    }

    [MenuItem("Window/Incremental Versioning Settings")]
    public static void ShowWindow()
    {
        GetWindow<BuildEditor>("Incremental Build Settings");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Current version", PlayerSettings.bundleVersion, GUILayout.MinWidth(900));
        EditorGUILayout.LabelField("Change current version in Player Settings");
        EditorGUILayout.Space();
        increment = EditorGUILayout.Toggle("Incremental Versioning", increment, GUILayout.Width(500));
        developmentVersion = EditorGUILayout.Toggle("Development Mode", developmentVersion, GUILayout.Width(900));

        if(GUILayout.Button("Increment Major"))
        {
            int[] a = ParseVersion();
            a[0]++;
            PlayerSettings.bundleVersion = CollapseVersion(a, developmentVersion);
        }

        if (GUILayout.Button("Increment Minor"))
        {
            int[] a = ParseVersion();
            a[1]++;
            PlayerSettings.bundleVersion = CollapseVersion(a, developmentVersion);
        }
    }

    public int[] ParseVersion()
    {
        string version = PlayerSettings.bundleVersion;
        string[] parsing = version.Split('.');
        int[] ret = new int[4];

        if (parsing.Length == 4)
        {
            int.TryParse(parsing[0], out ret[0]);
            if (!int.TryParse(parsing[1], out ret[1])) ret[1] = 1;
            if (!int.TryParse(parsing[2], out ret[2])) ret[2] = 1;
            //22b - b means development version
            if (parsing[3].Contains("b"))
            {
                string res = parsing[3].Remove(parsing[3].IndexOf('b'));
                int.TryParse(res, out ret[3]);
            }
            return ret;
        }
        else return new int[] { 0, 1, 1, 0 };
    }

    public string CollapseVersion(int[] ver, bool dev=true)
    {
        if (ver.Length >= 3) return string.Format("{0}.{1}.{2}.{3}{4}", ver[0], ver[1], ver[2], ver[3], dev ? "b" : "");
        
        else return "0.1.1.0" + (dev ? "b" : "");
    }

    public int callbackOrder
    {
        get
        {
            return 0;
        }
    }

    public void OnPreprocessBuild(BuildReport report)
    {

        int[] ver = ParseVersion();
        ver[2]++;

        PlayerSettings.bundleVersion = CollapseVersion(ver, developmentVersion);
        System.DateTime date = System.DateTime.Now;
        //27101036
        PlayerSettings.Android.bundleVersionCode = int.Parse(string.Format("{1:00}{0:00}{2:00}{3:00}", date.Day, date.Month + ((date.Year - 2018) * 12), date.Hour, date.Minute));
        Debug.Log("Building " + Application.productName + " version " + PlayerSettings.bundleVersion);
    }
}
