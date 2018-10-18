using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ExpenseType
{
    Maintenance, Salary, Loan
}

public class EndDayManager : MonoBehaviour {
    [Header("Panel Titles")]
    public string gameStartTitle = "Pre-Business Setup";
    public string dayStartTitle = "Start of Day {0}";
    public string dayEndTitle = "End of Day {0}";

    [Header("UI - General")]
    public Text titleText;
    public Text moneyText;
    public Toggle[] categoryButtons;

    [Header("UI - Finance")]
    public GameObject financeSection;
    public FinanceReportStruct[] financeReportRows;

    [Header("UI - Restock")]
    public GameObject restockSection;
    public Text restockTitle;
    public RestockStruct[] restockRows;
    public Button commitButton;

    [Header("UI - Prices")]
    public GameObject pricesSection;
    public PricesStruct[] pricesRows;

    [Header("UI - Forecast")]
    public GameObject forecastSection;
    static EndDayManager instance;

    int mode = 0;

    //Lists
    float[] gameRevenues = new float[5];
    int[] gameSaleCount = new int[5];
    int[] addedStocks = new int[5];

	// Use this for initialization
	void Start () {
        if (instance == null) instance = this;
        else Destroy(this);
        forecastSection.SetActive(false);
        gameObject.SetActive(false);
        
	}

    void PopulateRevenue()
    {
        float total = 0;
        foreach (var rev in gameRevenues) total += rev;
        for (int i = 0; i < financeReportRows.Length; i++)
        {
            if (i == 0)
            {
                financeReportRows[i].label.text = "Total Revenues";
                financeReportRows[i].value.text = string.Format("${0:N0}", total);
            }
            else if (MarketManager.IsGameAvailable((GameType)(i - 1)))
            {
                financeReportRows[i].label.text = string.Format("- {0}", MarketManager.GetGameNames((GameType)(i - 1)));
                financeReportRows[i].value.text = string.Format("({1}) ${0:N0}", gameRevenues[i - 1], gameSaleCount[i - 1]);
            }
            else
            {
                financeReportRows[i].label.text = "";
                financeReportRows[i].value.text = "";
            }
        }
    }

    void PopulateExpense()
    {
        float total = 0;
        //foreach (var rev in gameRevenues) total += rev;
        for (int i = 0; i < financeReportRows.Length; i++)
        {
            if (i == 0)
            {
                financeReportRows[i].label.text = "Total Expenses";
                financeReportRows[i].value.text = string.Format("${0:N0}", total);
            }/*
            else if (MarketManager.IsGameAvailable((GameType)(i - 1)))
            {
                financeReportRows[i].label.text = string.Format("- {0}", MarketManager.GetGameNames((GameType)(i - 1)));
                financeReportRows[i].value.text = string.Format("${0:N0}", gameRevenues[i - 1]);
            }*/
            else
            {
                financeReportRows[i].label.text = "";
                financeReportRows[i].value.text = "";
            }
        }
    }

    void PopulateInitialRestock()
    {
        for (int i = 0; i < restockRows.Length; i++)
        {
            if (MarketManager.IsGameAvailable(i))
            {
                restockRows[i].gameName.transform.parent.gameObject.SetActive(true);
                restockRows[i].gameName.text = string.Format("{0}", MarketManager.GetGameNames((GameType)i));
                restockRows[i].stockText.text = string.Format("{0:N0}", Logistics.GetStock(i));
                restockRows[i].priceText.text = "$0";
                restockRows[i].stockSlider.value = 0;
                restockRows[i].stockSlider.maxValue = Logistics.Capacity - Logistics.GetStock(i);
            }
            else
            {
                restockRows[i].gameName.transform.parent.gameObject.SetActive(false);
            }
        }
    }

    void PopulatePrices(bool init = false)
    {
        for (int i = 0; i < pricesRows.Length; i++)
        {
            if (MarketManager.IsGameAvailable(i))
            {
                pricesRows[i].gameName.transform.parent.gameObject.SetActive(true);
                pricesRows[i].gameName.text = string.Format("{0}", MarketManager.GetGameNames((GameType)i));
                UpdatePricesUI(i);
            }
            else
            {
                pricesRows[i].gameName.transform.parent.gameObject.SetActive(false);
            }
        }
    }
    //Finance Report
    public void ChangePage(int page)
    {
        /*
         * Pages:
         * 0 Revenue Report
         * 1 Expense Report
         * 2 Overtime
         * 3 Restock
         * 4 Forecast 
         * 5 Prices
         */

        //Hide all sections
        financeSection.SetActive(false);
        restockSection.SetActive(false);
        pricesSection.SetActive(false);
        //TODO do the same for Overtime and Forecast
        switch (page)
        {
            case 0:
                {
                    financeSection.SetActive(true);
                    PopulateRevenue();
                    break;
                }
            case 1:
                {
                    financeSection.SetActive(true);
                    PopulateExpense();
                    break;
                }
            case 3:
                {
                    restockSection.SetActive(true);
                    UpdateAllRestockUI();
                    break;
                }
            case 5:
                {
                    pricesSection.SetActive(true);
                    PopulatePrices();
                    break;
                }
            case 2:
            case 4:
            default:
                {
                    break;
                }
        }
    }


    //Restocks
    public void OnIncrease(int id)
    {
        if (restockRows[id].stockSlider.value != restockRows[id].stockSlider.maxValue)
        {
            addedStocks[id]++;
            restockRows[id].stockSlider.value++;
        }
        UpdateRestockUI(id);
        
    }

    public void OnDecrease(int id)
    {
        addedStocks[id] = Mathf.Max(addedStocks[id] - 1, 0);
        restockRows[id].stockSlider.value = Mathf.Max(restockRows[id].stockSlider.value - 1, 0);
        UpdateRestockUI(id);

    }

    public void OnSlider1ValueChange(float val)
    {
        addedStocks[0] = Mathf.RoundToInt(val);
        UpdateRestockUI(0);
    }

    public void OnSlider2ValueChange(float val)
    {
        addedStocks[1] = Mathf.RoundToInt(val);
        UpdateRestockUI(1);
    }

    public void OnSlider3ValueChange(float val)
    {
        addedStocks[2] = Mathf.RoundToInt(val);
        UpdateRestockUI(2);
    }

    public void OnSlider4ValueChange(float val)
    {
        addedStocks[3] = Mathf.RoundToInt(val);
        UpdateRestockUI(3);
    }

    public void OnSlider5ValueChange(float val)
    {
        addedStocks[4] = Mathf.RoundToInt(val);
        UpdateRestockUI(4);
    }

    public void OnPricesValueChange(int id)
    {
        UpdatePricesUI(id);
    }

    public void OnCommit()
    {
        float totalCash = 0;
        for (int i = 0; i < restockRows.Length; i++)
        {
            if (MarketManager.IsGameAvailable(i) && addedStocks[i] > 0)
            {
                totalCash += addedStocks[i] * MarketManager.GetBuyPrice(i);
                //TODO use logistics
                Logistics.RestockGame((GameType)i, addedStocks[i]);

            }
        }

        GameManager.Cash -= totalCash;
        addedStocks = new int[5];

        PopulateInitialRestock();
    }

    public void OnNextDay()
    {
        if (mode == 0)
        {
            GameManager.NextDay(false);
            gameObject.SetActive(false);
            ShowStartDayPanel();
            mode = 1;
        }
        else
        {
            GameManager.NextDay(true);
            gameObject.SetActive(false);
            instance.gameRevenues = new float[5];
            instance.gameSaleCount = new int[5];
            mode = 0;
        }
    }

    private void UpdateAllRestockUI()
    {
        for (int i = 0; i < restockRows.Length; i++) if (MarketManager.IsGameAvailable(i)) UpdateRestockUI(i);
    }

    private void UpdateAllPricesUI()
    {
        for (int i = 0; i < pricesRows.Length; i++) if (MarketManager.IsGameAvailable(i)) UpdatePricesUI(i);
    }

    void UpdatePricesUI(int id)
    {
        float priceMod = 1 + pricesRows[id].priceSlider.value / 10;
        string format = pricesRows[id].priceSlider.value == 0 ? "{0:N0}% (${1:N0})" : "<color=yellow>{0:N0}%</color> (${1:N0})";
        pricesRows[id].priceText.text = string.Format(format, priceMod * 100, MarketManager.GetBaseSalePrice(id) * priceMod);
        MarketManager.SetSalePriceMod(id, priceMod);
    }

    void UpdateRestockUI(int id)
    {
        float totalPrice = 0;
        int totalStock = 0;

        if (addedStocks[id] != 0) restockRows[id].stockText.text = string.Format("{0:N0} ({1:+#;-#;0})", Logistics.GetStock((GameType)id), addedStocks[id]);
        else restockRows[id].stockText.text = string.Format("{0:N0}", Logistics.GetStock((GameType)id));
        restockRows[id].priceText.text = string.Format("${0:N0}", MarketManager.GetBuyPrice(id) * addedStocks[id]);
        for (int i = 0; i < addedStocks.Length; i++) {
            if (MarketManager.IsGameAvailable(i))
            {
                totalPrice += MarketManager.GetBuyPrice(i) * addedStocks[i];
                totalStock += addedStocks[i];
            }
        }
        instance.moneyText.text = string.Format("${0:N0}", GameManager.Cash - totalPrice);

        if (GameManager.Cash - totalPrice < 0)
            instance.moneyText.color = Color.red;
        else instance.moneyText.color = Color.white;

        if(Logistics.GetTotalStocks() + totalStock > Logistics.Capacity)
            restockTitle.text = string.Format("Restock <color=red>({0}/{1})</color>", Logistics.GetTotalStocks() + totalStock, Logistics.Capacity);
        else restockTitle.text = string.Format("Restock ({0}/{1})", Logistics.GetTotalStocks() + totalStock, Logistics.Capacity);

        if (Logistics.GetTotalStocks() + totalStock > Logistics.Capacity || GameManager.Cash - totalPrice < 0)
            instance.commitButton.interactable = false;
        else instance.commitButton.interactable = true;
        
    }

    public static void ShowEndDayPanel()
    {
        if (GameManager.Days < 1) instance.titleText.text = instance.gameStartTitle;
        else instance.titleText.text = string.Format(instance.dayEndTitle, GameManager.Days);
        instance.addedStocks = new int[5];
        instance.gameObject.SetActive(true);
        instance.categoryButtons[3].interactable = true;
        instance.categoryButtons[0].isOn = true;
        instance.ChangePage(0);

        instance.PopulateInitialRestock(); //Restock
        instance.moneyText.text = string.Format("${0:N0}", GameManager.Cash);
    }

    public static void ShowStartDayPanel()
    {
        ShowEndDayPanel();
        instance.categoryButtons[3].interactable = false;
        instance.titleText.text = string.Format(instance.dayStartTitle, GameManager.Days);
        //TODO Set Game Sale Price

    }

    public static void AddRevenue(GameType game, float amt)
    {
        instance.gameRevenues[(int)game] += amt;
        instance.gameSaleCount[(int)game]++;
    }

    public static void AddExpense(ExpenseType type, Departments dept, float amt)
    {

    }
	
	// Update is called once per frame
	void Update () {

	}
}

[System.Serializable]
public struct FinanceReportStruct
{
    public Text label, value;
}

[System.Serializable]
public struct RestockStruct
{
    public Text gameName, stockText, priceText;
    public Slider stockSlider;
}

[System.Serializable]
public struct PricesStruct
{
    public Text gameName, priceText;
    public Slider priceSlider;
}
