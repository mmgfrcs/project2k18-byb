﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
    public GameObject endDayPanel;
    public Text titleText, moneyText, nextDayButtonText;
    public Toggle[] categoryButtons;
    public Animator faderAnimator;

    [Header("UI - Finance")]
    public GameObject financeSection, netRevenue, expenseByDept;
    public FinanceReportStruct[] financeReportRows;
    public FinanceReportStruct[] netRevenueRows;
    public FinanceReportStruct[] expenseByDeptRows;
    public GameObject financeDeptMissingObject;

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
    public ForecastEntry[] forecastRows;

    [Header("UI - Loans")]
    public GameObject loanSection;
    public LoanStruct[] loanRows;
    public Text loanButtonText;
    public Button loanButton;

    [Header("UI - Overtime")]
    public GameObject overtimeSection;
    public OvertimeEntry[] overtimeRows;

    [Header("UI - Marketing")]
    public GameObject marketingSection;
    public Text businessAdType, businessAdCost;
    public Text newRecurringCustText, totalCostText;
    public Slider newRecurringCustSlider;
    public MarketingEntry[] marketingRows;
    internal int businessAdPos = 0;

    [Header("UI - Shop")]
    public GameObject shopSection;
    public ShopEntry[] shopRows;

    [Header("UI - Special Abilities")]
    public GameObject abilitiesSection;
    public SpecialAbilityOverviewEntry[] abilityOverviewRows;
    public SpecialAbilityTrainEntry[] abilityTrainRows;
    public Sprite emptySprite;

    static EndDayManager instance;
    public static bool IsPanelOpen { get; private set; }
    int mode = 0, toloan = 0;
    float oldMarketingMod = 0, newMarketingMod = 0, pendingMarketingCost;
    
    List<System.Tuple<string, string>> expenseString, expenseByDeptString;
    Dictionary<string, DepartmentBase> overtimeAssociation = new Dictionary<string, DepartmentBase>();

    //Lists
    float[] gameRevenues = new float[5];
    int[] restocksCopy = new int[5], restocksOrig = new int[5];
    List<ExpensesData> expenses = new List<ExpensesData>();

    int[] gameSaleCount = new int[5];
    int[] addedStocks = new int[5];

	// Use this for initialization
	void Start () {
        if (instance == null) instance = this;
        else Destroy(this);
        forecastSection.SetActive(false);
        endDayPanel.SetActive(false);
	}

    void InitializeExpense()
    {
        if (expenseString == null || expenseString.Count == 0)
        {
            financeReportRows[0].label.text = "Total Expenses";
            financeReportRows[0].value.text = string.Format("${0:N0}", 0);

            for (int i = 1; i < financeReportRows.Length; i++)
            {
                financeReportRows[i].label.text = "";
                financeReportRows[i].value.text = "";
            }
        }
        else
        {
            for (int i = 0; i < financeReportRows.Length; i++)
            {
                var content = expenseString[i];
                financeReportRows[i].label.text = content.Item1;
                financeReportRows[i].value.text = content.Item2;
            }
        }

        if (GameManager.IsDepartmentExists(Departments.Finance))
        {
            expenseByDept.SetActive(true);
            financeDeptMissingObject.SetActive(false);
            
            if (expenseByDeptString == null || expenseByDeptString.Count == 0)
            {
                for (int i = 0; i < expenseByDeptRows.Length; i++)
                {
                    expenseByDeptRows[i].label.text = "";
                    expenseByDeptRows[i].value.text = "";
                }
            }
            else
            {
                for (int i = 0; i < expenseByDeptRows.Length; i++)
                {
                    expenseByDeptRows[i].label.text = expenseByDeptString[i].Item1;
                    expenseByDeptRows[i].value.text = expenseByDeptString[i].Item2;
                }
            }
        }
        else
        {
            expenseByDept.SetActive(false);
            financeDeptMissingObject.SetActive(true);
        }

        netRevenue.SetActive(false);
    }

    void InitializeOvertime()
    {
        overtimeAssociation = new Dictionary<string, DepartmentBase>();
        int dept = 2;
        for(int i = 0; i < overtimeRows.Length; i++)
        {
            
            DepartmentBase db = null;
            while(db == null)
            {
                if (dept > 9) break;
                if (GameManager.IsDepartmentExists((Departments)dept))
                {
                    GameObject deptGo = GameManager.GetDeptObject((Departments)dept);
                    if (deptGo != null)
                    {
                        db = deptGo.GetComponent<DepartmentBase>();
                        if (db.overtimeEffect == null) db = null;
                    }
                }
                
                dept++;
            }
            if(db != null)
            {
                overtimeAssociation.Add(db.departmentName, db);
                overtimeRows[i].gameObject.SetActive(true);
                overtimeRows[i].departmentName.text = db.departmentName;
                overtimeRows[i].overtimeEffect.text = db.overtimeEffect;
                overtimeRows[i].overtimeToggle.isOn = db.Overtime;
            }
            else overtimeRows[i].gameObject.SetActive(false);

            db = null;
        }
    }

    void InitializeSpecialAbilities()
    {
        for (int i = 0; i < abilityOverviewRows.Length; i++) {
            if (GameManager.IsDepartmentExists((Departments)(i+2)))
            {
                DepartmentBase dept = GameManager.GetDeptScript((Departments)(i+2));
                abilityOverviewRows[i].deptName.text = dept.departmentName;
                for(int j = 0; j < abilityOverviewRows[i].abilities.Length; j++)
                {
                    if (j >= dept.abilities.Count) abilityOverviewRows[i].abilities[j].sprite = emptySprite;
                    else abilityOverviewRows[i].abilities[j].sprite = dept.abilities[j].abilityIcon;
                }
            }
            else abilityOverviewRows[i].gameObject.SetActive(false);
        }

        for(int i = 0; i < abilityTrainRows.Length; i++)
        {
            SpecialAbility[] abilities = MarketManager.GetSpecialAbilities();
            if (i < abilities.Length )//&& abilities[i].AbilityDiscovered && abilities[i].trainable)
            {
                abilityTrainRows[i].abilityName.text = abilities[i].abilityName;
                abilityTrainRows[i].abilityDescription.text = abilities[i].abilityDescription;
                abilityTrainRows[i].abilityIcons.sprite = abilities[i].abilityIcon;
                abilityTrainRows[i].abilityTrainProgress.gameObject.SetActive(false);// = abilities[i].abilityName;

                string durationText;
                if (abilities[i].trainTimeHours <= 12) durationText = string.Format("{0:N0}h", abilities[i].trainTimeHours);
                else durationText = string.Format("{0:N0}", Mathf.CeilToInt(abilities[i].trainTimeHours / 12f));
                abilityTrainRows[i].trainDuration.text = durationText;
            }
            else abilityTrainRows[i].gameObject.SetActive(false);
        }
    }

    void PopulateExpense()
    {
        expenseString = new List<System.Tuple<string, string>>();
        float total = 0;
        foreach (var rev in expenses) total += rev.amount + rev.amountMod;

        //Group expenses by type
        List<System.Tuple<string, float>> expenseGroup = new List<System.Tuple<string, float>>();
        float totalGroup = 0;

        List<ExpensesData> salaryExpense = expenses.FindAll(x => x.type == ExpenseType.Salary);
        foreach (var sub in salaryExpense) totalGroup += sub.amount + sub.amountMod;
        ExpensesData maintenanceExpense = expenses.Find(x => x.type == ExpenseType.Maintenance);
        ExpensesData loanExpense = expenses.Find(x => x.type == ExpenseType.Loan);

        if (maintenanceExpense != null) expenseGroup.Add(System.Tuple.Create("Maintenance", maintenanceExpense.amount + maintenanceExpense.amountMod));
        if (totalGroup != 0) expenseGroup.Add(System.Tuple.Create("Salary", totalGroup));
        if(loanExpense != null) expenseGroup.Add(System.Tuple.Create("Loans", loanExpense.amount + loanExpense.amountMod));

        expenseString.Add(System.Tuple.Create("Total Expenses", string.Format("${0:N0}", total)));
        for (int i = 1; i < financeReportRows.Length; i++)
        {

            if (i <= expenseGroup.Count)
            {
                expenseString.Add(
                    System.Tuple.Create(string.Format("- {0}", expenseGroup[i - 1].Item1),
                    string.Format("${0:N0}", expenseGroup[i - 1].Item2)
                ));
            }
            else
            {
                expenseString.Add(System.Tuple.Create("", ""));
                financeReportRows[i].label.text = "";
                financeReportRows[i].value.text = "";
            }
            
        }

        expenseByDeptString = new List<System.Tuple<string, string>>();

        expenseByDeptString.Add(System.Tuple.Create("Salary by Department", ""));

        for (int i = 1; i < expenseByDeptRows.Length; i++)
        {
            if (i <= salaryExpense.Count)
            {
                expenseByDeptString.Add(System.Tuple.Create(
                    string.Format("- {0}", salaryExpense[i - 1].department.ToString()),
                    string.Format("${0:N0}", salaryExpense[i - 1].amount + salaryExpense[i - 1].amountMod)
                ));
            }
            else
            {
                expenseByDeptString.Add(System.Tuple.Create("", ""));
            }
        }

        InitializeExpense();
        
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

        if (GameManager.IsDepartmentExists(Departments.Finance))
        {
            netRevenue.SetActive(true);
            financeDeptMissingObject.SetActive(false);
            total = 0;
            for (int i = 1; i < netRevenueRows.Length; i++)
            {
                if (MarketManager.IsGameAvailable((GameType)(i - 1)))
                {
                    netRevenueRows[i].label.text = string.Format("- {0}", MarketManager.GetGameNames((GameType)(i - 1)));
                    float net = gameRevenues[i - 1] - MarketManager.GetBuyPrice(i - 1) * restocksCopy[i - 1];
                    if(net >=0) netRevenueRows[i].value.text = string.Format("${0:N0}", net);
                    else netRevenueRows[i].value.text = string.Format("<color=#ff6666>-${0:N0}</color>", -net);
                    total += net;
                }
                else
                {
                    netRevenueRows[i].label.text = "";
                    netRevenueRows[i].value.text = "";
                }
            }

            netRevenueRows[0].label.text = "Net Revenues";
            if (total >= 0) netRevenueRows[0].value.text = string.Format("${0:N0}", total);
            else netRevenueRows[0].value.text = string.Format("<color=#ff6666>-${0:N0}</color>", -total);
        }
        else
        {
            netRevenue.SetActive(false);
            financeDeptMissingObject.SetActive(true);
        }

        expenseByDept.SetActive(false);

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
                restockRows[i].stockSlider.maxValue = Logistics.GetCapacity() - Logistics.GetStock(i);
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

    void PopulateForecast()
    {
        Forecaster forecaster = GameManager.GetDeptScript(Departments.Forecaster) as Forecaster;
        for (int i = 0; i < pricesRows.Length; i++)
        {
            if (MarketManager.IsGameAvailable(i))
            {
                forecastRows[i].gameObject.SetActive(true);
                forecastRows[i].gameTitle.text = MarketManager.GetGameNames((GameType)i);
                forecastRows[i].nextDayForecast.text = string.Format("{0:N0} ~ {1:N0}%", 
                    Mathf.Round(MarketManager.GetSpecificDemandWeight(i, GameManager.Days) * forecaster.ForecastMinFactor * 10) * 10,
                    Mathf.Round(MarketManager.GetSpecificDemandWeight(i, GameManager.Days) * forecaster.ForecastMaxFactor * 10) * 10);

                forecastRows[i].nextWeekForecast.text = string.Format("{0:N0} ~ {1:N0}%",
                    Mathf.Round(MarketManager.GetSpecificDemandWeight(i, GameManager.Days + 7) * forecaster.ForecastMinFactor * 10) * 10,
                    Mathf.Round(MarketManager.GetSpecificDemandWeight(i, GameManager.Days + 7) * forecaster.ForecastMaxFactor * 10) * 10);

                float weight = 0;
                for(int j = GameManager.Days; j < GameManager.Days + 7; j++)
                {
                    weight += MarketManager.GetSpecificDemandWeight(i, j);
                }
                weight /= 7;
                forecastRows[i].weekAvgForecast.text = string.Format("{0:N0} ~ {1:N0}%",
                    Mathf.Round(weight * forecaster.ForecastMinFactor * 10) * 10,
                    Mathf.Round(weight * forecaster.ForecastMaxFactor * 10) * 10);
                
            }
            else
            {
                forecastRows[i].gameObject.SetActive(false);
            }
        }
        
    }

    void PopulateLoans()
    {
        int tCount = 0;
        for (int i = 0; i < loanRows.Length; i++)
        {
            var data = MarketManager.GetLoanData(i);
            if (!data.taken)
            {
                loanRows[i].loanName.text = data.loanName;
                loanRows[i].interest.text = string.Format("{0:N0}%", data.interest);
                loanRows[i].amountLbl.text = "Amount";
                loanRows[i].amountValue.text = string.Format("${0:N0}", data.amount);
                loanRows[i].durationLbl.text = "Duration";
                loanRows[i].monthlyCost.text = string.Format("${0:N0}", data.amount * (1 + data.interest / 100) / data.term);
            }
            else
            {
                tCount++;
                loanRows[i].loanName.text = data.loanName;
                loanRows[i].interest.text = string.Format("{0:N0}%", data.interest);
                loanRows[i].amountLbl.text = "Rem. Amt.";
                loanRows[i].amountValue.text = string.Format("${0:N0}", data.amount);
                loanRows[i].durationLbl.text = "Rem. Term";
                loanRows[i].durationValue.text = string.Format("{0} days", data.term);
                loanRows[i].monthlyCost.text = string.Format("${0:N0}", data.amount / data.term);
                
            }
        }

        if(tCount == 0) foreach (var rows in loanRows) rows.amountLbl.transform.parent.GetComponent<Toggle>().interactable = true;
        else foreach (var rows in loanRows) rows.amountLbl.transform.parent.GetComponent<Toggle>().interactable = false;
    }
    
    #region Marketing Button Action
    public void OnBusinessAdNext()
    {
        businessAdPos++;
        UpdateMarketingUI();
    }

    public void OnBusinessAdPrev()
    {
        businessAdPos--;
        UpdateMarketingUI();
    }

    public void OnGameAdNext(int id)
    {
        marketingRows[id].pos++;
        UpdateMarketingUI();
    }

    public void OnGameAdPrev(int id)
    {
        marketingRows[id].pos--;
        UpdateMarketingUI();
    }

    public void OnCustomerRatioChange(float val)
    {
        newMarketingMod = val;
        UpdateMarketingUI();
    }

    void UpdateMarketingUI()
    {
        int i = 0;
        pendingMarketingCost = 0;
        foreach(var entry in marketingRows)
        {
            entry.gameTitle.text = MarketManager.GetGameNames(i++);
            entry.adType.text = MarketManager.GetGameAdName(Mathf.Abs(entry.pos) % 5);
            pendingMarketingCost += MarketManager.GetGameAdPrice(Mathf.Abs(entry.pos) % 5);
        }
        businessAdType.text = MarketManager.GetBusinessAdName(Mathf.Abs(businessAdPos) % 5);
        businessAdCost.text = string.Format("${0:N0}", MarketManager.GetBusinessAdPrice(Mathf.Abs(businessAdPos) % 5));
        pendingMarketingCost += MarketManager.GetBusinessAdPrice(Mathf.Abs(businessAdPos) % 5);

        newRecurringCustSlider.minValue = GameManager.BaseVisitChance;
        newRecurringCustSlider.maxValue = GameManager.BaseVisitChance * 1.5f;
        newRecurringCustText.text = string.Format("{0:N1}%", newRecurringCustSlider.value * 100);
        float ratio = (newRecurringCustSlider.value - newRecurringCustSlider.minValue) / (newRecurringCustSlider.maxValue - newRecurringCustSlider.minValue);
        pendingMarketingCost += ratio * 500;

        totalCostText.text = string.Format("${0:N0}", pendingMarketingCost);
    }
    #endregion

    void PayExpenses()
    {
        for(int i = 0; i < expenses.Count; i++)
        {
            GameManager.Cash -= expenses[i].amount + expenses[i].amountMod;
            
        }

        LoanData data = MarketManager.GetLoanData(toloan);
        if (data.taken)
        {
            MarketManager.PayLoanDaily();
            data = MarketManager.GetLoanData(toloan);
            AddExpense(ExpenseType.Loan, Departments.Start, data.amount / data.term);
        }

        PopulateExpense();
    }

    void PopulateShop()
    {
        foreach(var ent in shopRows)
        {
            if(ent.ItemBought || GameManager.CompanyLevel < ent.requiredLevel) ent.gameObject.SetActive(false);
            else ent.gameObject.SetActive(true);
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
         * 6 Loan
         * 7 Marketing
         * 8 Shop
         * 9 Special Abilities
         */
        instance.moneyText.text = string.Format("${0:N0}", GameManager.Cash);
        //Hide all sections
        financeSection.SetActive(false);
        overtimeSection.SetActive(false);
        restockSection.SetActive(false);
        pricesSection.SetActive(false);
        forecastSection.SetActive(false);
        loanSection.SetActive(false);
        marketingSection.SetActive(false);
        shopSection.SetActive(false);
        abilitiesSection.SetActive(false);
        
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
                    InitializeExpense();
                    break;
                }
            case 2:
                {
                    overtimeSection.SetActive(true);
                    break;
                }
            case 3:
                {
                    restockSection.SetActive(true);
                    UpdateAllRestockUI();
                    break;
                }
            case 4:
                {
                    forecastSection.SetActive(true);
                    PopulateForecast();
                    //UpdateAllRestockUI();
                    break;
                }
            case 5:
                {
                    pricesSection.SetActive(true);
                    PopulatePrices();
                    break;
                }
            case 6:
                {
                    loanSection.SetActive(true);
                    break;
                }
            case 7:
                {
                    marketingSection.SetActive(true);
                    UpdateMarketingUI();
                    break;
                }
            case 8:
                {
                    PopulateShop();
                    shopSection.SetActive(true);
                    break;
                }
            case 9:
                {
                    abilitiesSection.SetActive(true);
                    InitializeSpecialAbilities();
                    break;
                }
            default:
                {
                    break;
                }
        }
    }

    #region Restock Actions
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

    void OnSliderValueChange(float val, int id)
    {
        addedStocks[id] = Mathf.RoundToInt(val);
        UpdateRestockUI(id);
    }

    public void OnSlider1ValueChange(float val)
    {
        OnSliderValueChange(val, 0);
    }

    public void OnSlider2ValueChange(float val)
    {
        OnSliderValueChange(val, 1);
    }

    public void OnSlider3ValueChange(float val)
    {
        OnSliderValueChange(val, 2);
    }

    public void OnSlider4ValueChange(float val)
    {
        OnSliderValueChange(val, 3);
    }

    public void OnSlider5ValueChange(float val)
    {
        OnSliderValueChange(val, 4);
    }

    public void OnCommit()
    {
        restocksOrig = new int[5];
        float totalCash = 0;
        for (int i = 0; i < restockRows.Length; i++)
        {
            if (MarketManager.IsGameAvailable(i) && addedStocks[i] > 0)
            {
                totalCash += addedStocks[i] * MarketManager.GetBuyPrice(i);
                restocksOrig[i] += addedStocks[i];
                Logistics.RestockGame((GameType)i, addedStocks[i]);

            }
        }

        GameManager.Cash -= totalCash;
        addedStocks = new int[5];

        PopulateInitialRestock();
    }
    #endregion

    #region Prices Button Actions
    public void OnPricesValueChange(int id)
    {
        UpdatePricesUI(id);
    }

    
    #endregion

    #region Loans Button Actions
    public void OnLoanValueChange(bool val)
    {
        if (val) loanButton.interactable = true;
        else loanButton.interactable = false;
    }

    public void OnLoanValueChange(int id)
    {
        toloan = id;
    }
    #endregion

    public void OnOvertimeToggle(int id)
    {
        DepartmentBase db = overtimeAssociation[overtimeRows[id].departmentName.text];
        db.Overtime = overtimeRows[id].overtimeToggle.isOn;
    }

    public void OnLoanCommit()
    {
        LoanData data = MarketManager.GetLoanData(toloan);
        GameManager.Cash += data.amount;
        loanButton.interactable = false;
        MarketManager.TakeLoan(toloan);
        //Add expense
        AddExpense(ExpenseType.Loan, Departments.Start, data.amount * (1 + data.interest / 100) / data.term);
        instance.moneyText.text = string.Format("${0:N0}", GameManager.Cash);
        PopulateLoans();
    }

    public void OnNextDay()
    {
        if (mode == 0)
        {
            faderAnimator.Play("FadeIn");
            GameManager.NextDay(false);
            mode = 1;
        }
        else
        {
            IsPanelOpen = false;
            GameManager.NextDay(true, endDayPanel);
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

        if(Logistics.GetTotalStocks() + totalStock > Logistics.GetCapacity())
            restockTitle.text = string.Format("Restock <color=red>({0}/{1})</color>", Logistics.GetTotalStocks() + totalStock, Logistics.GetCapacity());
        else restockTitle.text = string.Format("Restock ({0}/{1})", Logistics.GetTotalStocks() + totalStock, Logistics.GetCapacity());

        if (Logistics.GetTotalStocks() + totalStock > Logistics.GetCapacity() || GameManager.Cash - totalPrice < 0)
            instance.commitButton.interactable = false;
        else instance.commitButton.interactable = true;
        
    }

    void CheckDepartment()
    {
        //Forecast
        if (GameManager.IsDepartmentFunctional(Departments.Forecaster)) categoryButtons[4].interactable = true;
        else categoryButtons[4].interactable = false;
        //Loans
        if (GameManager.IsDepartmentFunctional(Departments.Finance)) categoryButtons[6].interactable = true;
        else categoryButtons[6].interactable = false;
        //Marketing
        if (GameManager.IsDepartmentFunctional(Departments.Marketing)) instance.categoryButtons[7].interactable = true;
        else categoryButtons[7].interactable = false;
        //Special Abilities
        if (GameManager.IsDepartmentFunctional(Departments.HRD)) categoryButtons[9].interactable = true;
        else categoryButtons[9].interactable = false;
        //Shop
        //instance.categoryButtons[8].interactable = false; 
    }

    public static void ShowEndDayPanel()
    {
        instance.InitializeExpense();
        IsPanelOpen = true;
        EventManager.EndEvent();
        if (GameManager.Days < 1) instance.titleText.text = instance.gameStartTitle;
        else
        {
            instance.titleText.text = string.Format(instance.dayEndTitle, GameManager.Days);
            if (DaytimeManager.TimeHour > 15) instance.PayExpenses();
        }
        instance.restocksCopy = instance.restocksOrig;
        instance.addedStocks = new int[5];
        instance.endDayPanel.SetActive(true);
        instance.categoryButtons[3].interactable = true;
        instance.categoryButtons[0].isOn = true;
        instance.CheckDepartment();
        instance.ChangePage(0);

        instance.nextDayButtonText.text = "Next Day";
        //instance.PopulateForecast();
        instance.PopulateLoans();
        instance.PopulateInitialRestock(); //Restock
        instance.InitializeOvertime();
        
    }

    public static void ShowStartDayPanel()
    {
        EventManager.RunEvent();
        ShowEndDayPanel();
        instance.nextDayButtonText.text = "Start Day";
        instance.categoryButtons[3].interactable = false;
        instance.titleText.text = string.Format(instance.dayStartTitle, GameManager.Days);
        instance.faderAnimator.Play("FadeOut");
        
        //TODO Set Game Sale Price

    }

    public static void AddRevenue(GameType game, float amt)
    {
        if (GameManager.isInDemoMode) return;
        instance.gameRevenues[(int)game] += amt;
        instance.gameSaleCount[(int)game]++;
    }
    
    public static void AddExpense(ExpenseType type, Departments dept, float amt, float amtMod = 0)
    {
        if (GameManager.isInDemoMode) return;
        RemoveExpense(type, dept);

        ExpensesData data = new ExpensesData()
        {
            type = type,
            department = dept,
            amount = amt,
            amountMod = amtMod
        };
        instance.expenses.Add(data);
    }
	
    public static void ModifyExpense(ExpenseType type, Departments dept, float amtMod)
    {
        if (GameManager.isInDemoMode) return;
        ExpensesData foundData = instance.expenses.Find(x => x.type == type && x.department == dept);

        if (foundData != null)
        {
            instance.expenses.Remove(foundData);
            instance.expenses.Add(new ExpensesData()
            {
                type = type,
                department = dept,
                amount = foundData.amount + amtMod,
                amountMod = foundData.amountMod
            });
        }
    }

    public static void RemoveExpense(ExpenseType type, Departments dept)
    {
        if (GameManager.isInDemoMode) return;
        ExpensesData foundData = instance.expenses.Find(x => x.type == type && x.department == dept);
        if (foundData != null) instance.expenses.Remove(foundData);
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

[System.Serializable]
public struct ForecastStruct
{
    public Text gameName, adType, adPrice, forecast;

}

[System.Serializable]
public struct LoanStruct
{
    public Text loanName, amountLbl, amountValue, interest, durationLbl, durationValue, monthlyCost;
}

public class ExpensesData
{
    public ExpenseType type;
    public Departments department;
    public int startDay, duration;
    public float amount, amountMod;
}