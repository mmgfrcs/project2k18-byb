using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public enum Departments
{
    Start, Showcase, Cashier, CustService, Logistics, Marketing, Finance
}

public class GameManager : MonoBehaviour {

    public bool mainMenuMode;

    [Header("Customer")]
    public string customerNameFile = "custnames";

    [Header("Mechanics")]
    public int level;
    public float startingMoney;
    public float customerSpawnTime = 6f;
    public float baseVisitChance = 0.5f;
    public float maintenanceCost = 100f;
    public float newCustomerRatio = 1;

    [Header("Objects")]
    public Transform[] startPos;
    public Transform[] wanderPos;
    public GameObject[] customerObjects;
    public List<GameObject> deptObjects;
    public StockTabPanel stockTab;
    public Transform cameraPivot;

    [Header("Settings"), Range(0.1f,3f)]
    public float panSensitivity = 1f;
    [Range(0.1f, 3f)]
    public float rotateSensitivity = 1f;
    [Range(0.1f, 3f)]
    public float pinchSensitivity = 1f;

    [Header("UI - General")]
    public Text moneyText;
    public Text timeText, stockText, levelText;

    [Header("UI - Selection Panel")]
    public GameObject customerSelect, departmentSelect;
    public InfoPanel customerSelectContents;
    public DepartmentPanel departmentSelectContents;
    public Button selectHireBtn, selectFireBtn;

    [Header("Animators")]
    public Animator selectStatusAnim;
    public Animator stockAnimator, pausePanelAnim, gameFaderAnim;
    
    internal static float NetCustomerSpawnTime { get { return instance.customerSpawnTime / Mathf.Max(VisitChance, 0.001f); } }
    internal static float VisitChance { get { return instance.baseVisitChance + instance.visitChanceMod; } }
    internal static float BaseVisitChance { get { return instance.baseVisitChance; } }
    internal static float Cash { get; private set; }
    internal static int Days { get; private set; }
    internal static int GameLevel { get; private set; }
    internal static bool isInDemoMode { get { return instance != null ? instance.mainMenuMode : false; } }

    public static event System.Action OnNextDay, OnGameEnd;

    //Statistics
    public float NetIncome { get; private set; }
    public float TotalIncome { get; private set; }
    public float TotalExpenses { get; private set; }

    string[] custNames;
    float visitChanceMod = 0;
    public static GameManager instance;
    static Dictionary<Departments, DepartmentBase> depts = new Dictionary<Departments, DepartmentBase>();
    List<GameObject> recurringCust = new List<GameObject>();

    TKRotationRecognizer rt = new TKRotationRecognizer();
    TKPinchRecognizer pr = new TKPinchRecognizer();

    ISelectable selected;

    float t = 0;
    int cid = 1, custCount = 0, nameCount = 0;

    bool canSpawnCustomer = true, isSpawning, exiting;

    private void Awake()
    {
        depts = new Dictionary<Departments, DepartmentBase>();
    }

    // Use this for initialization
    void Start () {
        instance = this;
        print("GameManager instance change, Showcase mode is " + (instance.mainMenuMode ? "on" : "off"));

        if (!mainMenuMode)
        {
            
            GameLevel = 1;

            EndDayManager.AddExpense(ExpenseType.Maintenance, Departments.Start, maintenanceCost);

            Cash = startingMoney;

            rt.gestureRecognizedEvent += (i) =>
            {
                if (!EndDayManager.IsPanelOpen) cameraPivot.Rotate(Vector3.up, -rt.deltaRotation * rotateSensitivity, Space.World);
            };

            pr.gestureRecognizedEvent += (i) =>
            {
                if (!EndDayManager.IsPanelOpen) Camera.main.transform.Translate(Vector3.forward * 2 * pr.deltaScale * pinchSensitivity);
            };
            TouchKit.addGestureRecognizer(rt);
            TouchKit.addGestureRecognizer(pr);

            DaytimeManager.OnDayEnd += DaytimeManager_OnDayEnd;
            EventManager.RunStartEvent();

            gameFaderAnim.Play("FadeOut");
        }
    }
    
    // Update is called once per frame
    void Update () {
        if (exiting) return;
        if(!depts.ContainsKey(Departments.Logistics) || !depts.ContainsKey(Departments.Cashier) || !depts.ContainsKey(Departments.Showcase))
        {
            Debug.LogError("Cannot load game: Department requirements not met\nRequired Logistics, Cashier and Showcase");
            enabled = false;
        }

        if(canSpawnCustomer && !isSpawning) t += Time.deltaTime;
        if (t >= NetCustomerSpawnTime && canSpawnCustomer)
        {
            t = 0;
            isSpawning = true;
            StartCoroutine(SpawnTime());
        }
        if (!mainMenuMode)
        {
            timeText.text = string.Format("{0:N0}:{1:00}", DaytimeManager.TimeHour, DaytimeManager.TimeMinute);

            stockTab.totalStocks.text = string.Format("{0:N0}/{1:N0}", Logistics.GetTotalStocks(), Logistics.GetCapacity());
            stockTab.totalStocksBar.maxValue = Logistics.GetCapacity();
            stockTab.totalStocksBar.value = Logistics.GetTotalStocks();
            stockTab.totalStocksFill.color = Color.HSVToRGB((1 - (float)Logistics.GetTotalStocks() / Logistics.GetCapacity()) / 3.6f, 1, 1);
            for (int i = 0; i < 5; i++)
            {
                if (MarketManager.IsGameAvailable((GameType)i))
                {
                    stockTab.gameTitles[i].text = MarketManager.GetGameNames((GameType)i);
                    stockTab.gameStocks[i].text = Logistics.GetStock(i).ToString("n0");
                    if (i > 0) stockTab.separators[i - 1].SetActive(true);
                }
                else
                {
                    stockTab.gameTitles[i].text = "";
                    stockTab.gameStocks[i].text = "";
                    if (i > 0) stockTab.separators[i - 1].SetActive(false);
                }
            }

            //Input (touch)
            if (Time.timeScale != 0 && !EndDayManager.IsPanelOpen)
            {
                var touch = Input.touches;
                if (touch.Length == 1 && touch[0].deltaPosition.magnitude > 0.2f)
                {
                    Vector3 delta = new Vector3(touch[0].deltaPosition.x, 0, touch[0].deltaPosition.y);
                    cameraPivot.Translate(delta * -Time.deltaTime * 0.4f * panSensitivity);
                }
                else if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit))
                    {

                        ISelectable sel = hit.collider.GetComponent<ISelectable>();

                        if (sel != null && !(sel is Showcase))
                        {
                            if (selected == null) selectStatusAnim.Play("Open");
                            DeselectAll();

                            selected = sel;
                            selected.Select();
                        }
                        else
                        {
                            if (selected != null) selectStatusAnim.Play("Close");
                            DeselectAll();
                        }

                    }
                }
            }
            if (selected != null)
            {
                if (selected is Customer)
                {
                    var selectedCustomer = selected as Customer;
                    customerSelect.SetActive(true);
                    departmentSelect.SetActive(false);
                    customerSelectContents.NameText.text = selectedCustomer.custName;
                    customerSelectContents.Text1Text.text = selectedCustomer.CurrentActionName + " " + (selectedCustomer.CurrentActionName == "Shopping" ? MarketManager.GetGameNames(selectedCustomer.GameDemand) : "");
                    customerSelectContents.Bar1Slider.maxValue = 100;
                    customerSelectContents.Bar1Slider.value = selectedCustomer.Happiness;
                    customerSelectContents.Progress1Slider.maxValue = selectedCustomer.MaxProgressTime;
                    customerSelectContents.Progress1Slider.value = selectedCustomer.CurrentProgressTime;
                    customerSelectContents.stat1.text = selectedCustomer.Visits.ToString("n0");
                    customerSelectContents.stat2.text = string.Format("${0:n0}", selectedCustomer.TotalSpendings);
                }
                else if (selected is DepartmentBase)
                {
                    var selectedDept = selected as DepartmentBase;
                    departmentSelect.SetActive(true);
                    customerSelect.SetActive(false);
                    departmentSelectContents.departmentName.text = selectedDept.departmentName;
                    departmentSelectContents.trustBar.maxValue = 100;
                    departmentSelectContents.trustBar.value = selectedDept.CurrentTrust;
                    if (selected is Cashier || selected is CustomerService) departmentSelectContents.workSpeedLbl.text = "Work Speed";
                    else departmentSelectContents.workSpeedValue.text = "Effectiveness";
                    departmentSelectContents.workSpeedValue.text = string.Format("{0:N0}%", selectedDept.WorkSpeed * 100);
                }
            }
        }
    }

    IEnumerator SpawnTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(NetCustomerSpawnTime + Random.Range(NetCustomerSpawnTime * -0.1f, NetCustomerSpawnTime * 0.1f));
            Transform start = MathRand.Pick(startPos);
            int choice = MathRand.WeightedPick(new float[] { recurringCust.Count, newCustomerRatio });
            if (choice == 1)
            {
                Customer c = Instantiate(MathRand.Pick(customerObjects), start.position, start.rotation).GetComponent<Customer>();
                if (mainMenuMode) c.actionWeight = new float[] { 3, 6, 1 };
                c.custName = AssignCustomerName();
            }
            else CustomerReturn(MathRand.Pick(recurringCust)); //else we return it
            isSpawning = false;
            custCount++;
            break;
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        pausePanelAnim.Play("Open");
    }

    public void UnpauseGame()
    {
        Time.timeScale = 1;
        pausePanelAnim.Play("Close");
    }

    public void ExitGame()
    {
        print("Preparing for exit...");
        exiting = true;
        instance = null;
        OnGameEnd?.Invoke();

        LoadingScreenManager.nextSceneName = "Menu";
        Time.timeScale = 1;
        StartCoroutine(WaitExitAnim());
    }

    public void SaveGameState()
    {

    }

    public void LoadGameState()
    {

    }

    private void DeselectAll()
    {
        if(selected != null) selected.Deselect();
        selected = null;
    }

    private string AssignCustomerName()
    {
        
        if (custNames == null)
        {
            TextAsset text;
            text = Resources.Load<TextAsset>(customerNameFile);
            if (text == null) return "Customer " + RequestID();
            custNames = text.text.Split('\n');
            MathRand.Shuffle(ref custNames);
            return AssignCustomerName();
        }
        else
        {
            return custNames[nameCount++];
        }
        
    }

    public void OpenStockPanel()
    {
        var state = stockAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Closed") || state.IsName("Close")) stockAnimator.Play("Open");
        else stockAnimator.Play("Close");
    }

    private void DaytimeManager_OnDayEnd()
    {
        canSpawnCustomer = false;
        StartCoroutine(ProcessDayEnd());
    }

    IEnumerator ProcessDayEnd()
    {
        DaytimeManager.PauseDaytime();
        if (Days > 0)
        {
            yield return new WaitWhile(() => { return custCount > 0; });
            if (selected != null) DeselectAll();
            stockAnimator.Play("Closed");

            gameFaderAnim.Play("FadeIn");
            yield return new WaitForSeconds(1f);
            EndDayManager.ShowEndDayPanel();
            gameFaderAnim.Play("FadeOut");
        }
        else
        {
            stockAnimator.Play("Closed");
            EndDayManager.ShowEndDayPanel();
        }
    }

    IEnumerator WaitExitAnim()
    {
        print("Exit coroutine called");
        gameFaderAnim.Play("FadeIn");
        yield return new WaitForSeconds(1f);
        print("Exit coroutine complete");
        SceneManager.LoadScene("Loading");
    }

    bool IsInfoPanelClosed()
    {
        return selectStatusAnim.GetCurrentAnimatorStateInfo(0).IsName("Closed") || selectStatusAnim.GetCurrentAnimatorStateInfo(0).IsName("Close");
    }

    int bankruptDay = 0;
    bool bankrupt = false;
    
    public static void ActivateSelected()
    {
        if (instance.selected != null) (instance.selected as DepartmentBase).OnAbilityUse();
    }

    public static void OnMainMenu()
    {
        instance.ExitGame();
    }

    public static void CheckConditions()
    {
        if (Cash < 0)
        {
            instance.bankruptDay++;
            if (instance.bankruptDay > 1)
            {
                if (instance.bankrupt)
                {
                    //Game Over: It's no longer savable
                    EndGame();
                }
                else
                {
                    //It can still be saved
                    EventManager.RunEvent(EventRunMode.Bankruptcy);
                    instance.bankrupt = true;
                }
                
            }
        }
        else instance.bankruptDay = 0;

        if(instance.level == 1)
        {
            if (Days > 30) EndGame();
            else if (instance.TotalIncome >= 2500) EventManager.RunEvent(EventRunMode.Victory);
        }
    }

    public static void AdjustCash(float amount)
    {
        Cash += amount;
        instance.NetIncome += amount;
        instance.TotalExpenses -= Mathf.Min(amount, 0);
        instance.TotalIncome += Mathf.Max(amount, 0);
        print($"Cashflow: {amount}. Net {instance.NetIncome}, Income {instance.TotalIncome}, Expense {instance.TotalExpenses}");
    }

    public static void SaveBusiness()
    {
        Cash = 0;
        foreach(var dept in GetAllDeptScripts())
        {
            dept.AdjustTrust(dept.CurrentTrust / -2);
        }
    }

    /// <summary>
    /// Ends the game abruptly. Counts as a defeat by default, but the game can be abruptly ended with a victory as well
    /// </summary>
    /// <param name="victory">True ends the game with a victory, defeat otherwise. Defaults to false</param>
    public static void EndGame(bool victory=false)
    {
        EventManager.RunEvent(EventRunMode.Defeat);
    }

    public static void SetVisitChanceMod(float mod, bool adjust = false)
    {
        if (adjust) instance.visitChanceMod += mod;
        else instance.visitChanceMod = mod;
    }

    public static void RegisterDepartment(Departments dept, DepartmentBase deptScript)
    {
        print("Department Registered: " + dept);
        depts.Add(dept, deptScript);
    }
    
    public static void RegisterMap(Transform[] startPos, Transform[] wanderPos)
    {
        throw new System.NotImplementedException("RegisterMap is not implemented");
    }

    public static void NextDay(bool isDay, GameObject panel = null)
    {
        if (isDay)
        {
            instance.StartCoroutine(instance.StartDay(panel));
        }
        else
        {
            Days++;
            DaytimeManager.AdvanceTimeTo(8);
            OnNextDay?.Invoke();
        }
        
    }

    IEnumerator StartDay(GameObject panel)
    {
        gameFaderAnim.Play("FadeIn");
        yield return new WaitForSeconds(1f);
        panel.SetActive(false);
        gameFaderAnim.Play("FadeOut");
        yield return new WaitForSeconds(1f);
        instance.canSpawnCustomer = true;
        DaytimeManager.UnpauseDaytime();
        
    }

    public static int RequestID()
    {
        return instance.cid++;
    }

    Dictionary<Departments, List<Transform>> intr = new Dictionary<Departments, List<Transform>>();
    List<Transform> wPos = new List<Transform>();

    public static Transform GetWander()
    {
        if (instance.wPos.Count == 0) instance.wPos.AddRange(instance.wanderPos);

        var wand = instance.wPos[0];
        instance.wPos.RemoveAt(0);
        return wand;
    }

    public static bool IsDepartmentExists(Departments dept)
    {
        return depts.ContainsKey(dept);
    }

    public static bool IsDepartmentFunctional(Departments dept)
    {
        if (!IsDepartmentExists(dept)) return false;
        return depts[dept].IsFunctional;
    }

    public static Transform GetInteractable(Departments dept)
    {
        if (dept == Departments.Start) return MathRand.Pick(instance.startPos);
        if (!IsDepartmentFunctional(dept)) return null;

        if (!instance.intr.ContainsKey(dept)) instance.intr.Add(dept, new List<Transform>());

        if(instance.intr[dept].Count == 0) instance.intr[dept].AddRange(MathRand.ChooseSetDependent(depts[dept].interactablePosition.ToArray(), depts[dept].interactablePosition.Count));
        
        var interact = instance.intr[dept][0];
        instance.intr[dept].RemoveAt(0);
        return interact;
    }

    public static Transform GetLookDirection(Departments dept)
    {
        if (!IsDepartmentFunctional(dept)) return null;
        return depts[dept].lookDirection;
    }

    public static GameObject GetDeptObject(Departments dept)
    {
        if (dept == Departments.Start) return null;
        return GetDeptScript(dept).gameObject;
    }

    public static DepartmentBase GetDeptScript(Departments dept)
    {
        if (dept == Departments.Start) return null;
        return depts[dept];
    }

    public static List<DepartmentBase> GetAllDeptScripts()
    {
        List<DepartmentBase> deptsList = new List<DepartmentBase>();
        foreach (var i in depts) deptsList.Add(i.Value);
        return deptsList;
    }

    public static float CommitTransaction(Customer cust, GameType game)
    {
        if (!instance.mainMenuMode)
        {
            if (Logistics.GetStock(game) > 0)
            {
                //Success
                cust.CommitTransaction(true);
                float revenue = MarketManager.GetSalePrice(game);
                AdjustCash(revenue);
                EndDayManager.AddRevenue(game, revenue);
                Logistics.ExpendGame(game);
                return revenue;
            }
            else
            {
                //Failure
                cust.CommitTransaction(false);
                return 0;
            }
        }
        else
        {
            cust.CommitTransaction(true);
            return 0;
        }
    }

    public static void CustomerLeave(GameObject custObj)
    {
        if (!instance.mainMenuMode && instance.selected as Customer == custObj.GetComponent<Customer>())
        {
            instance.selectStatusAnim.Play("Close");
            instance.DeselectAll();
        }
        instance.recurringCust.Add(custObj);
        custObj.SetActive(false);
        custObj.transform.position = new Vector3(-1000, -1000, -1000);
        instance.custCount--;
    }

    void CustomerReturn(GameObject custObj)
    {
        Transform start = MathRand.Pick(startPos);
        instance.recurringCust.Remove(custObj);
        custObj.transform.position = start.position;
        custObj.transform.rotation = start.rotation;
        custObj.SetActive(true);
        custObj.GetComponent<Customer>().ChooseAction();
    }
}