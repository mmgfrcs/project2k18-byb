using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MarketManager : MonoBehaviour {
    [Header("Market Dynamics")]
    public MarketKeypoint[] demandKeypoints;
    public MarketKeypoint[] saleKeypoints, buyKeypoints;
    public string[] adNames = new string[] { "None", "Radio", "Flyers", "TV", "Balloon" }, 
        businessAdNames = new string[] { "None", "Radio", "Flyers", "TV", "Balloon" };
    public float[] adPrices = new float[] { 0, 25, 50, 100, 200 },
        businessAdPrices = new float[] { 0, 100, 200, 400, 800 };
    public float[] demandMod = new float[] { 1, 1, 1, 1, 1 }, permanentMod = new float[5];

    [Header("Loans")]
    public LoanData[] loans = new LoanData[3];

    [Header("Special Abilities")]
    public GameObject specialAbilityObject;
    
    LoanData currLoan = null;
    List<AnimationCurve> demands, salePrices, buyPrices;
    SpecialAbility[] specialAbilities;
    float[] pricesMod = new float[] { 1, 1, 1, 1, 1 };
    int[] adLevel = new int[5];
    Dictionary<GameType, string> gameNames = new Dictionary<GameType, string>()
    {
        {GameType.GameA, "Edwin's" },
        {GameType.GameB, "Rius'" },
        {GameType.GameC, "Derrick's" },
        {GameType.GameD, "Virya's" },
        {GameType.GameE, "Asyraf's" },
    };
    static MarketManager instance;

    private void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        specialAbilities = specialAbilityObject.GetComponents<SpecialAbility>();
        print("Market Manager - Loaded Special Abilities: " + specialAbilities.Length);
        
        demands = new List<AnimationCurve>();
        //Each Game has its own curve, so loop that too
        for (int i = 0; i < gameNames.Count; i++)
        {
            AnimationCurve curve = new AnimationCurve();
            foreach (var key in demandKeypoints)
            {
                curve.AddKey(new Keyframe(key.day, key.dynamics[i]));
            }
            demands.Add(CurveModifier.SetCurveLinear(curve));
        }
        
        salePrices = new List<AnimationCurve>();
        for (int i = 0; i < gameNames.Count; i++)
        {
            AnimationCurve curve = new AnimationCurve();
            foreach (var key in saleKeypoints)
            {
                curve.AddKey(new Keyframe(key.day, key.dynamics[i]));
            }
            salePrices.Add(CurveModifier.SetCurveLinear(curve));
        }

        buyPrices = new List<AnimationCurve>();
        for (int i = 0; i < gameNames.Count; i++)
        {
            AnimationCurve curve = new AnimationCurve();
            foreach (var key in buyKeypoints)
            {
                curve.AddKey(new Keyframe(key.day, key.dynamics[i]));
            }
            buyPrices.Add(CurveModifier.SetCurveLinear(curve));
        }
    }

    public static LoanData GetLoanData(int id)
    {
        if (instance.currLoan != null && instance.currLoan.loanName == instance.loans[id].loanName) return instance.currLoan;
        return instance.loans[id];
    }

    public static SpecialAbility[] GetSpecialAbilities()
    {
        return instance.specialAbilities;
    }

    public static void StartAbilityTraining(int abilityid)
    {

    }

    public static void TakeLoan(int id)
    {
        instance.currLoan = GetLoanData(id);
        instance.currLoan.amount *= (1 + instance.currLoan.interest / 100);
        instance.currLoan.taken = true;
    }

    public static void PayLoanDaily()
    {
        instance.currLoan.term--;
        PayLoan(false, instance.currLoan.amount / instance.currLoan.term);
    }

    public static void PayLoan(bool sweep, float amount = 0)
    {
        if (sweep) instance.currLoan = null;
        else instance.currLoan.amount -= amount;

        if (instance.currLoan.term == 0) instance.currLoan = null;
    }

    public static void SetDemands(int gameId, float demand)
    {
        if (!GameManager.isInDemoMode)
            instance.demandMod[gameId] = demand;
    }

    public static float GetGameAdPrice(int adId)
    {
        if (GameManager.isInDemoMode) return 0;
        return instance.adPrices[adId];
    }

    public static string GetGameAdName(int adId)
    {
        return instance.adNames[adId];
    }

    public static float GetBusinessAdPrice(int adId)
    {
        if (GameManager.isInDemoMode) return 0;
        return instance.businessAdPrices[adId];
    }

    public static string GetBusinessAdName(int adId)
    {
        return instance.businessAdNames[adId];
    }

    public static AdData GetAdDataForGame(int gameId)
    {
        if (GameManager.isInDemoMode) return new AdData();
            return new AdData() {
            adLevel = instance.adLevel[gameId],
            adName = GetGameAdName(instance.adLevel[gameId]),
            adPrice = instance.adPrices[instance.adLevel[gameId]] };
    }

    public static int GetDemands()
    {
        if (GameManager.isInDemoMode) return MathRand.WeightedPick(new float[] { 1, 1, 1 });
        List<float> demandList = new List<float>();
        for(int i = 0; i < instance.gameNames.Count; i++) demandList.Add(instance.demands[i].Evaluate(GameManager.Days));
        return MathRand.WeightedPick(demandList);
        
        //return MathRand.WeightedPick(instance.demandArr);
    }

    internal static float GetSpecificDemandWeight(int game, int day)
    {
        float totalWeight = 0;
        for (int i = 0; i < instance.gameNames.Count; i++) totalWeight += instance.demands[i].Evaluate(day);
        return instance.demands[game].Evaluate(day) / totalWeight;
    }

    public static float GetSalePrice(GameType game)
    {
        return GetSalePrice((int)game);
    }

    public static float GetSalePrice(int game)
    {
        if (GameManager.isInDemoMode) return GetBaseSalePrice(game);
        return GetBaseSalePrice(game) * instance.pricesMod[game];
        //return instance.salePricesArr[game];
    }

    public static void SetSalePriceMod(GameType game, float mod)
    {
        SetSalePriceMod((int)game, mod);
    }

    public static void SetSalePriceMod(int game, float mod)
    {
        instance.pricesMod[game] = Mathf.Clamp(mod, 0f, 2f);
    }

    public static float GetSalePriceMod(GameType game)
    {
        return GetSalePriceMod((int)game);
    }

    public static float GetSalePriceMod(int game)
    {
        return instance.pricesMod[game];
    }

    public static float GetBaseSalePrice(GameType game)
    {
        return GetBaseSalePrice((int)game);
    }

    public static float GetBaseSalePrice(int game)
    {
        if (GameManager.isInDemoMode) return 0;
        return instance.salePrices[game].Evaluate(GameManager.Days);
        //return instance.salePricesArr[game];
    }

    public static float GetBuyPrice(GameType game)
    {
        return GetBuyPrice((int)game);
    }

    public static float GetBuyPrice(int game)
    {
        return instance.buyPrices[game].Evaluate(GameManager.Days);
        //return instance.buyPricesArr[game];
    }

    public static bool IsGameAvailable(GameType game)
    {
        return IsGameAvailable((int)game);
    }

    public static bool IsGameAvailable(int game)
    {
        if (GameManager.isInDemoMode)
        {
            if (game < 3) return true;
            else return false;
        }

        return game < instance.gameNames.Count && 
            instance.demands[game].Evaluate(GameManager.Days) != 0;

        //if (game > 2) return instance.demandArr.Length <= 4 && instance.demandArr[game] != 0;
        //else return instance.demandArr[game] != 0;
    }

    public static string GetGameNames(GameType game)
    {
        if (GameManager.isInDemoMode) return "";
        return instance.gameNames[game];
    }

    public static string GetGameNames(int game)
    {
        return instance.gameNames[(GameType)game];
    }
}

public static class CurveModifier
{
    public static AnimationCurve SetCurveLinear(AnimationCurve curve)
    {
        for (int i = 0; i < curve.keys.Length; ++i)
        {
            float intangent = 0;
            float outtangent = 0;
            bool intangent_set = false;
            bool outtangent_set = false;
            Vector2 point1;
            Vector2 point2;
            Vector2 deltapoint;
            Keyframe key = curve[i];

            if (i == 0)
            {
                intangent = 0; intangent_set = true;
            }

            if (i == curve.keys.Length - 1)
            {
                outtangent = 0; outtangent_set = true;
            }

            if (!intangent_set)
            {
                point1.x = curve.keys[i - 1].time;
                point1.y = curve.keys[i - 1].value;
                point2.x = curve.keys[i].time;
                point2.y = curve.keys[i].value;

                deltapoint = point2 - point1;

                intangent = deltapoint.y / deltapoint.x;
            }
            if (!outtangent_set)
            {
                point1.x = curve.keys[i].time;
                point1.y = curve.keys[i].value;
                point2.x = curve.keys[i + 1].time;
                point2.y = curve.keys[i + 1].value;

                deltapoint = point2 - point1;

                outtangent = deltapoint.y / deltapoint.x;
            }

            key.inTangent = intangent;
            key.outTangent = outtangent;
            curve.MoveKey(i, key);
        }

        return curve;
    }
}

[System.Serializable]
public struct MarketKeypoint
{
    public int day;
    public float[] dynamics;
}

public struct AdData
{
    public string adName;
    public float adPrice;
    public int adLevel;
}
[System.Serializable]
public class LoanData
{
    public string loanName;
    public float amount, interest;
    public int term;
    internal bool taken;
}