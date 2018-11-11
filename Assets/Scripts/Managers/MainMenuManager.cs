using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour {
    
    public void OnPlay()
    {
        LoadingScreenManager.nextSceneName = "Game";
        SceneManager.LoadScene("Loading");
    }

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
