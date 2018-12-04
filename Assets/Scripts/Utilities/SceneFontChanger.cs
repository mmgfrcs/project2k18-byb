using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFontChanger : MonoBehaviour {
    public Font globalFont;
	// Use this for initialization
	void Start () {
        GameObject[] gameObjects = SceneManager.GetActiveScene().GetRootGameObjects();
        foreach(GameObject gameObject in gameObjects)
        {
            foreach(Text txt in gameObject.GetComponentsInChildren<Text>(true))
            {
                txt.font = globalFont;
            }
        }
        enabled = false;
	}
}
