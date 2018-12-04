using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum Departments
{
    Start, Showcase, Cashier, CustService, Logistics, Marketing, Finance, HRD, Forecaster, Research
}

public class GameManager : MonoBehaviour {

    public bool mainMenuMode;

    [Header("Customer")]
    public string customerNameFile = "custnames";

    [Header("Mechanics")]
    public float startingMoney;
    public float customerSpawnTime = 6f;
    public float baseVisitChance = 0.5f;
    public float maintenanceCost = 100f;
    public float newCustomerRatio = 1;
    public float[] baseXPPerLevel = { 1000 };
    public float nextXPPerLevel = 1000;

    [Header("Objects")]
    public Transform[] startPos;
    public Transform[] wanderPos;
    public GameObject[] customerObjects;
    public StatusTabPanel[] statusTabDepts;
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

    [Header("UI - Canvas")]
    public Canvas stockCanvas;

    [Header("Animators")]
    public Animator selectStatusAnim;
    public Animator statusAnimator;
    public Animator stockAnimator, pausePanelAnim, gameFaderAnim;
    
    internal static float VisitChance { get { return instance.baseVisitChance + instance.visitChanceMod; } }
    internal static float Cash { get; set; }
    internal static int Days { get; private set; }
    internal static int CompanyLevel { get; private set; }
    internal static float CurrentXP { get; private set; }
    internal static bool isInDemoMode { get { return instance.mainMenuMode; } }
    internal static float NextXP { get {
            if (_xp.Length > CompanyLevel) return _xp[_xp.Length - 1] + _nxp * (CompanyLevel - _xp.Length);
            else return _xp[CompanyLevel];
        }
    }

    public static event System.Action OnLevelUp, OnNextDay;

    string[] custNames;
    float visitChanceMod = 0;
    static GameManager instance;
    static float[] _xp;
    static float _nxp;
    Text selectedHireText;
    static Dictionary<Departments, DepartmentBase> depts = new Dictionary<Departments, DepartmentBase>();
    List<GameObject> recurringCust = new List<GameObject>();

    TKRotationRecognizer rt = new TKRotationRecognizer();
    TKPinchRecognizer pr = new TKPinchRecognizer();

    ISelectable selected;

    float t = 0;
    int cid = 1, custCount = 0, nameCount = 0;

    bool canSpawnCustomer = true, isSpawning;

    private void Awake()
    {
        depts = new Dictionary<Departments, DepartmentBase>();
    }

    // Use this for initialization
    void Start () {
        if (instance == null || instance.mainMenuMode != mainMenuMode)
        {
            instance = this;
            print("GameManager instance change, Showcase mode is " + (instance.mainMenuMode ? "on" : "off"));
        }
        else Destroy(this);

        if (!mainMenuMode)
        {

            _xp = baseXPPerLevel;
            _nxp = nextXPPerLevel;
            CompanyLevel = 1;

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

            selectedHireText = selectHireBtn.GetComponentInChildren<Text>();

            gameFaderAnim.Play("FadeOut");
        }
    }
    
    // Update is called once per frame
    void Update () {
        if(!depts.ContainsKey(Departments.Logistics) || !depts.ContainsKey(Departments.Cashier) || !depts.ContainsKey(Departments.Showcase))
        {
            Debug.LogError("Cannot load game: Department requirements not met\nRequired Logistics, Cashier and Showcase");
            enabled = false;
        }

        if(canSpawnCustomer && !isSpawning) t += Time.deltaTime;
        if (t >= customerSpawnTime && canSpawnCustomer)
        {
            t = 0;
            isSpawning = true;
            StartCoroutine(SpawnTime());
        }
        if (!mainMenuMode)
        {
            levelText.text = CompanyLevel.ToString("n0");
            moneyText.text = string.Format("({1}) ${0:N1}", Cash, CurrentXP);
            timeText.text = string.Format("{0:N0}:{1:00}", DaytimeManager.TimeHour, DaytimeManager.TimeMinute);

            int i = 2;
            foreach (var status in statusTabDepts)
            {
                DepartmentBase db = null;
                while (db == null)
                {
                    if (i <= 10)
                    {
                        if (depts.ContainsKey((Departments)i)) db = depts[(Departments)i];
                        i++;
                    }
                    else break;
                }

                if (db != null)
                {
                    status.department = db;
                    status.departmentName.text = db.departmentName;
                    status.trustSlider.maxValue = 100;
                    status.trustSlider.value = db.CurrentTrust;
                    status.employeeSlider.wholeNumbers = true;
                    status.employeeSlider.maxValue = db.MaximumStaff;
                    status.employeeSlider.value = db.CurrentStaff;
                }
                else status.gameObject.SetActive(false);
            }

            stockTab.totalStocks.text = string.Format("{0:N0}/{1:N0}", Logistics.GetTotalStocks(), Logistics.GetCapacity());
            stockTab.totalStocksBar.maxValue = Logistics.GetCapacity();
            stockTab.totalStocksBar.value = Logistics.GetTotalStocks();
            stockTab.totalStocksFill.color = Color.HSVToRGB((1 - (float)Logistics.GetTotalStocks() / Logistics.GetCapacity()) / 3.6f, 1, 1);
            for (i = 0; i < 5; i++)
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

                            if (selected is DepartmentBase) departmentSelectContents.overtimeToggle.isOn = (selected as DepartmentBase).Overtime;
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
                    departmentSelectContents.staffs.text = selectedDept.CurrentStaff + "/" + selectedDept.MaximumStaff;
                    departmentSelectContents.salary.text = string.Format("${0:N0}", selectedDept.salary * selectedDept.CurrentStaff);
                    if (selected is Cashier || selected is CustomerService) departmentSelectContents.workSpeedLbl.text = "Work Speed";
                    else departmentSelectContents.workSpeedValue.text = "Effectiveness";
                    departmentSelectContents.workSpeedValue.text = string.Format("{0:N0}%", selectedDept.WorkSpeed * 100);

                    selectedHireText.text = string.Format("Hire (${0:N0})", selectedDept.StaffHireCost);
                    if (Cash < selectedDept.StaffHireCost || selectedDept.CurrentStaff >= selectedDept.MaximumStaff) selectHireBtn.interactable = false;
                    else selectHireBtn.interactable = true;
                    if (selectedDept.CurrentStaff > 0) selectFireBtn.interactable = true;
                    else selectFireBtn.interactable = false;
                }
            }
        }
    }

    IEnumerator SpawnTime()
    {
        while (true)
        {
            if (MathRand.WeightedPick(new float[] { Mathf.Max(0, (1 - VisitChance) * 100), Mathf.Min(100, VisitChance * 100) }) == 1 || mainMenuMode)
            {
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
            else yield return new WaitForSeconds(1);
        }
    }

    public void OnToggleOvertime(bool overtime)
    {
        if(selected != null && selected is DepartmentBase)
        {
            (selected as DepartmentBase).Overtime = overtime;
        }
    }

    public void OnHireSelectedStaff()
    {
        DepartmentBase select = selected as DepartmentBase;
        Cash -= select.StaffHireCost;
        select.AddStaff();
    }

    public void OnFireSelectedStaff()
    {
        DepartmentBase select = selected as DepartmentBase;
        select.RemoveStaff();
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

    public void OpenStatusPanel()
    {
        var state = statusAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Closed") || state.IsName("Close")) statusAnimator.Play("Open");
        else statusAnimator.Play("Close");
        stockCanvas.sortingOrder = -1;
    }

    public void OpenStockPanel()
    {
        var state = stockAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Closed") || state.IsName("Close")) stockAnimator.Play("Open");
        else stockAnimator.Play("Close");
        stockCanvas.sortingOrder = 1;
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
            stockCanvas.sortingOrder = -1;
            if (selected != null) DeselectAll();
            statusAnimator.Play("Closed");
            stockAnimator.Play("Closed");

            gameFaderAnim.Play("FadeIn");
            yield return new WaitForSeconds(1f);
            EndDayManager.ShowEndDayPanel();
            gameFaderAnim.Play("FadeOut");
        }
        else
        {
            stockCanvas.sortingOrder = -1;
            statusAnimator.Play("Closed");
            stockAnimator.Play("Closed");
            EndDayManager.ShowEndDayPanel();
        }
    }

    bool IsInfoPanelClosed()
    {
        return selectStatusAnim.GetCurrentAnimatorStateInfo(0).IsName("Closed") || selectStatusAnim.GetCurrentAnimatorStateInfo(0).IsName("Close");
    }

    public static void AddXP(float amount)
    {
        CurrentXP += amount;
        if (CurrentXP >= NextXP)
        {
            CompanyLevel++;
            OnLevelUp?.Invoke();
        }
    }

    public static void SetVisitChanceMod(float mod, bool adjust = false)
    {
        if (adjust) instance.visitChanceMod += mod;
        else instance.visitChanceMod = mod;
    }

    public static void BuyItem(ShopEntry entry)
    {
        entry.gameObject.SetActive(false);
        Cash -= entry.itemPrice;

        //switch-case Item ID
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

        if(instance.intr[dept].Count == 0) instance.intr[dept].AddRange(MathRand.ChooseSetDependent(depts[dept].interactablePosition, depts[dept].interactablePosition.Length));
        
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

    public static int GetTotalStaffs()
    {
        int staff = 0;
        foreach(var dept in depts)
        {
            staff += dept.Value.CurrentStaff;
        }
        return staff;
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
                Cash += revenue;
                CurrentXP += revenue * 2;
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