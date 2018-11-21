using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameType
{
    GameA, GameB, GameC, GameD, GameE
}

public class Logistics : DepartmentBase {

    public int startingCapacity = 50;
    public int[] startingStocks = new int[] { 20, 10, 5 };

    public static int Capacity { get; private set; }

    int[] currentStocks = new int[5];
    int[] pendingStocks = new int[5];
    static Logistics instance;

	// Use this for initialization
	protected override void Start () {
        if (instance == null) instance = this;
        else Destroy(this);
        base.Start();

        trustDrainRate = 0.1f;
        CurrentStaff = 2;
        departmentName = "Logistics";
        overtimeEffect = "Increase obtained bought supplies";
        GameManager.RegisterDepartment(Departments.Logistics, this);
        Capacity = startingCapacity;

        currentStocks = startingStocks;

        while(GetTotalStocks() > Capacity)
        {
            ExpendGame(0);
            ExpendGame(1);
            ExpendGame(2);
            ExpendGame(3);
            ExpendGame(4);
        }
	}

    public static int GetStock(GameType game)
    {
        return GetStock((int)game);
    }

    public static int GetStock(int game)
    {
        return instance.currentStocks[game];
    }

    public static int GetTotalStocks()
    {
        int total = 0;
        foreach (int stocks in instance.currentStocks) total += stocks;
        return total;
    }

    public static void DeliverOrder()
    {
        for(int i = 0; i < instance.pendingStocks.Length; i++)
        {
            instance.currentStocks[i] += instance.pendingStocks[i];
        }
        instance.pendingStocks = new int[5];
    }

    public static void RestockGame(int game, int amount = 1, bool order = false)
    {
        if(order) instance.pendingStocks[game] = instance.pendingStocks[game] + Mathf.Abs(amount);
        else instance.currentStocks[game] = instance.currentStocks[game] + Mathf.Abs(amount);
    }

    public static void RestockGame(GameType game, int amount = 1, bool order = false)
    {
        RestockGame((int)game, amount, order);
    }

    public static void ExpendGame(int game, int amount = 1)
    {
        instance.currentStocks[game] = Mathf.Max(instance.currentStocks[game] - Mathf.Abs(amount), 0);
    }

    public static void ExpendGame(GameType game, int amount = 1)
    {
        ExpendGame((int)game, amount);
    }
}
