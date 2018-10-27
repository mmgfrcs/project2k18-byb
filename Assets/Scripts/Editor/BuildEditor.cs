using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildEditor : EditorWindow, IPreprocessBuildWithReport {
    bool developmentVersion = true, increment = true;

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
        int[] ret = new int[3];

        if (parsing.Length == 3)
        {
            int.TryParse(parsing[0], out ret[0]);
            if (int.TryParse(parsing[1], out ret[1])) ret[1] = 1;
            //22b - b means development version
            if (parsing[2].Contains("b"))
            {
                string res = parsing[2].Remove(parsing[2].IndexOf('b'));
                int.TryParse(res, out ret[2]);
            }
            return ret;
        }
        else return new int[] { 0, 1, 1 };
    }

    public string CollapseVersion(int[] ver, bool dev=true)
    {
        if (ver.Length >= 3) return string.Format("{0}.{1}.{2}{3}", ver[0], ver[1], ver[2], dev ? "b" : "");
        
        else return "0.1.1" + (dev ? "b" : "");
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
        PlayerSettings.Android.bundleVersionCode = int.Parse(string.Format("{0}{1}{2}", date.Day, date.Month, date.Hour, date.Minute));
        Debug.Log("Building " + Application.productName + " version " + PlayerSettings.bundleVersion);
    }
}
