using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    public UnityEngine.UI.Text versionText;
    public Animator faderAnimator;
    
    public void OnPlay()
    {
        LoadingScreenManager.nextSceneName = "Game";
        StartCoroutine(WaitPlayAnim());
    }

    IEnumerator WaitPlayAnim()
    {
        faderAnimator.Play("FadeIn");
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Loading");
    }

	// Use this for initialization
	void Start () {
        versionText.text = "Version " + Application.version;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
