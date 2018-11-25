using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventManager : MonoBehaviour {

    static EventManager instance;

	// Use this for initialization
	void Start () {
        if (instance == null) instance = this;
        else Destroy(this);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    internal static void RunEvent()
    {

    }

    internal static void RollEvent()
    {

    }
}
