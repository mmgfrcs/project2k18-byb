using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenManager : MonoBehaviour {
    public string[] tips = new string[]
    {
        "This is a collaboration project",
        "Try keeping customers happy first before earning profits",
        "Loans can bite you in the end if not managed properly"
    };
    public static string nextSceneName = "Menu";
    public Animator faderAnim;
    public Text tipsText;
    public Slider loadSlider;
	AsyncOperation ao;
    string chosenTxt;
	// Use this for initialization
	void Start () {
		ao = SceneManager.LoadSceneAsync(nextSceneName);
		ao.allowSceneActivation = false;
        chosenTxt = MathRand.Pick(tips);
        StartCoroutine(LoadSceneActive());
	}

	IEnumerator LoadSceneActive()
	{
		yield return new WaitForSeconds(4f);
		while (true)
		{
            
            if (ao.progress >= 0.9f) break;
			else yield return new WaitForEndOfFrame();
		}
        faderAnim.Play("FadeOut");
        yield return new WaitForSeconds(1f);
        ao.allowSceneActivation = true;
    }
    float prog = 0;
    // Update is called once per frame
    void Update () {

	}
}
