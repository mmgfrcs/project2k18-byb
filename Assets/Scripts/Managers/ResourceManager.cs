using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceManager : MonoBehaviour {

    public enum GameType
    {
        None, GameA, GameB, GameC
    }

    int[] stocks = new int[4];
    static ResourceManager instance;

	// Use this for initialization
	void Start () {
        stocks = new int[] { Random.Range(100, 500), Random.Range(30, 240), Random.Range(10, 100), 0 };
        if (instance == null) instance = this;
        else Destroy(this);
    }
	
	// Update is called once per frame
	void Update () {

	}

    public static int GetStock(GameType game)
    {
        return instance.stocks[(int)game];
    }

    public static void ExpendGame(GameType game, int amount = 1)
    {
        instance.stocks[(int)game] = Mathf.Max(instance.stocks[(int)game] - 1, 0);
    }
}
