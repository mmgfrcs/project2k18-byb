using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MarketManager : MonoBehaviour {

    public float[] baseDemands = new float[] { 6, 2, 1 };
    public float[] basePrices = new float[] { 8, 16, 24 };
    Dictionary<ResourceManager.GameType, string> gameNames = new Dictionary<ResourceManager.GameType, string>()
    {
        {ResourceManager.GameType.None, "Edwin's" },
        {ResourceManager.GameType.GameA, "Rius'" },
        {ResourceManager.GameType.GameB, "Derrick's" },
        {ResourceManager.GameType.GameC, "Virya's" }
    };
    static MarketManager instance;
    

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }
    
    // Update is called once per frame
    void Update () {

    }

	void MarketUpdate()
	{

    }

    public static int GetDemands()
    {
        return MathRand.WeightedPick(instance.baseDemands);
    }

    public static float GetPrices(ResourceManager.GameType game)
    {
        return instance.basePrices[(int)game];
    }

    public static string GetGameNames(ResourceManager.GameType game)
    {
        return instance.gameNames[game];
    }
}

[System.Serializable]
public struct MarketData
{
    
}