using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    public Text versionText;
    public Animator faderAnimator, canvasAnimator;

    public Text creditAppName, creditAppDesc;
    
    public void OnPlay()
    {
        LoadingScreenManager.nextSceneName = "Game";
        StartCoroutine(WaitPlayAnim());
    }

    public void OnOptions()
    {
        canvasAnimator.Play("Options");
    }

    public void OnOptionsBack()
    {
        canvasAnimator.Play("Main Menu (Opt)");
    }

    public void OnCredits()
    {
        canvasAnimator.Play("Credits");
    }

    public void OnCreditsBack()
    {
        canvasAnimator.Play("Main Menu (Cred)");
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

        if (Application.platform == RuntimePlatform.Android && Input.GetKeyDown(KeyCode.Escape)) OnExit();
        
    }

    public void OnExit()
    {
        Application.Quit();
    }
}
