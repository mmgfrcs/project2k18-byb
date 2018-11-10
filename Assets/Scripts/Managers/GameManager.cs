using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public enum Departments
{
    Start, Showcase, Cashier, CustService, Logistics, Marketing, HRD, Forecaster
}

public class GameManager : MonoBehaviour {

    [Header("Customer")]
    public string customerNameFile = "custnames";

    [Header("Mechanics")]
    public float startingMoney;
    public float customerSpawnTime = 6f;
    public float maintenanceCost = 100f;
    public float[] baseXPPerLevel = { 1000 };
    public float nextXPPerLevel = 1000;

    [Header("Objects")]
    public Transform[] startPos;
    public Transform[] wanderPos;
    public GameObject[] customerObjects;
    public Transform cameraPivot;

    [Header("Settings"), Range(0.1f,3f)]
    public float panSensitivity = 1f;
    [Range(0.1f, 3f)]
    public float rotateSensitivity = 1f;
    [Range(0.1f, 3f)]
    public float pinchSensitivity = 1f;

    [Header("UI")]
    public Text moneyText;
    public Text timeText, stockText, levelText;
    public GameObject customerSelect, departmentSelect;
    public Animator selectStatusAnim;
    public InfoPanel customerSelectContents;
    public DepartmentPanel departmentSelectContents;

    [Header("Panels")]
    public Canvas shopCanvas;
    public Animator shopAnimator, statusAnimator;

    internal static float Cash { get; set; }
    internal static int Days { get; private set; }
    internal static int CompanyLevel { get; private set; }
    internal static float CurrentXP { get; private set; }
    internal static float NextXP { get {
            if (_xp.Length > CompanyLevel) return _xp[_xp.Length - 1] + _nxp * (CompanyLevel - _xp.Length);
            else return _xp[CompanyLevel];
        }
    }

    public static event System.Action OnLevelUp;

    string[] custNames;
    static GameManager instance;
    static float[] _xp;
    static float _nxp;
    static Dictionary<Departments, DepartmentBase> depts = new Dictionary<Departments, DepartmentBase>();
    List<GameObject> recurringCust = new List<GameObject>();

    TKRotationRecognizer rt = new TKRotationRecognizer();
    TKPinchRecognizer pr = new TKPinchRecognizer();

    ISelectable selected;

    float t = 0;
    int cid = 1, custCount = 0, nameCount = 0;

    bool canSpawnCustomer = true;

    private void Awake()
    {
        depts = new Dictionary<Departments, DepartmentBase>();
    }

    // Use this for initialization
    void Start () {
        if (instance == null) instance = this;
        else Destroy(this);
        _xp = baseXPPerLevel;
        _nxp = nextXPPerLevel;
        CompanyLevel = 1;

        EndDayManager.AddExpense(ExpenseType.Maintenance, Departments.Start, maintenanceCost);

        Cash = startingMoney;

        rt.gestureRecognizedEvent += (i) => {
            if(canSpawnCustomer) cameraPivot.Rotate(Vector3.up, -rt.deltaRotation * rotateSensitivity, Space.World);
        };

        pr.gestureRecognizedEvent += (i) =>
        {
            if (canSpawnCustomer) Camera.main.transform.Translate(Vector3.forward * 2 * pr.deltaScale * pinchSensitivity);
        };
        TouchKit.addGestureRecognizer(rt);
        TouchKit.addGestureRecognizer(pr);

        DaytimeManager.OnDayEnd += DaytimeManager_OnDayEnd;
    }
    
    // Update is called once per frame
    void Update () {
        if(!depts.ContainsKey(Departments.Logistics) || !depts.ContainsKey(Departments.Cashier))
        {
            Debug.LogError("Cannot load game: Department requirements not met");
            enabled = false;
        }

        levelText.text = CompanyLevel.ToString("n0");

        if(canSpawnCustomer) t += Time.deltaTime;
        if (t >= customerSpawnTime && canSpawnCustomer)
        { 
            t = 0;
            Transform start = MathRand.Pick(startPos);
            int choice = MathRand.WeightedPick(new float[] { recurringCust.Count, 1 });
            if (choice == 1)
            {
                Customer c = Instantiate(MathRand.Pick(customerObjects), start.position, start.rotation).GetComponent<Customer>();
                c.custName = AssignCustomerName();
            }
            else CustomerReturn(MathRand.Pick(recurringCust)); //else we return it
            
            custCount++;
        }

        moneyText.text = string.Format("${0:N1}", Cash);
        timeText.text = string.Format("{0:N0}:{1:00}", DaytimeManager.TimeHour, DaytimeManager.TimeMinute);

        //Input (touch)
        if (DaytimeManager.IsRunning)
        {
            var touch = Input.touches;
            if (touch.Length == 1 && touch[0].deltaPosition.magnitude > 0.2f)
            {
                Vector3 delta = new Vector3(touch[0].deltaPosition.x, 0, touch[0].deltaPosition.y);
                cameraPivot.Translate(delta * -Time.deltaTime * 0.4f * panSensitivity);
            }
            else if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {

                    ISelectable sel = hit.collider.GetComponent<ISelectable>();

                    if (sel != null && !(sel is Showcase))
                    {
                        if(selected == null) selectStatusAnim.Play("Open");
                        DeselectAll();

                        selected = sel;
                        selected.Select();
                    }
                    else
                    {
                        selectStatusAnim.Play("Close");
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
                departmentSelectContents.salary.text = string.Format("${0:N0}", 0);
                departmentSelectContents.workSpeed.text = "100%";
            }
            /*
            infoPanelContents.NameText.text = selectedCustomer.custName;
            infoPanelContents.Text1Text.text = selectedCustomer.CurrentActionName + " " + (selectedCustomer.CurrentActionName == "Shopping" ? MarketManager.GetGameNames(selectedCustomer.GameDemand) : "");
            infoPanelContents.Bar1Slider.maxValue = 100;
            infoPanelContents.Bar1Slider.value = selectedCustomer.Happiness;
            infoPanelContents.Progress1Slider.maxValue = selectedCustomer.MaxProgressTime;
            infoPanelContents.Progress1Slider.value = selectedCustomer.CurrentProgressTime;
            */
            //infoPanelContents.Bar1Image.sprite = Hearts
        }
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

    public void OpenShopPanel()
    {
        var state = shopAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Closed") || state.IsName("Close")) shopAnimator.Play("Open");
        else shopAnimator.Play("Close");
        shopCanvas.sortingOrder = 1;
    }

    public void OpenStatusPanel()
    {
        var state = statusAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName("Closed") || state.IsName("Close")) statusAnimator.Play("Open");
        else statusAnimator.Play("Close");
        shopCanvas.sortingOrder = -1;
    }

    private void DaytimeManager_OnDayEnd()
    {
        canSpawnCustomer = false;
        StartCoroutine(ProcessDayEnd());
    }

    IEnumerator ProcessDayEnd()
    {
        DaytimeManager.PauseDaytime();
        yield return new WaitWhile(() => { return custCount > 0; });
        shopCanvas.sortingOrder = -1;
        shopAnimator.Play("Closed");
        statusAnimator.Play("Closed");
        EndDayManager.ShowEndDayPanel();

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

    public static void RegisterDepartment(Departments dept, DepartmentBase deptScript)
    {
        print("Department Registered: " + dept);
        depts.Add(dept, deptScript);
    }

    public static void RegisterMap(Transform[] startPos, Transform[] wanderPos)
    {

    }

    public static void NextDay(bool isDay)
    {
        if (isDay)
        {
            instance.canSpawnCustomer = true;
            DaytimeManager.UnpauseDaytime();
        }
        else
        {
            Days++;
            DaytimeManager.AdvanceTimeTo(8);
        }
        
    }

    public static int RequestID()
    {
        return instance.cid++;
    }

    static Dictionary<Departments, List<Transform>> intr = new Dictionary<Departments, List<Transform>>();
    static List<Transform> wPos = new List<Transform>();

    public static Transform GetWander()
    {
        if (wPos.Count == 0) wPos.AddRange(instance.wanderPos);

        var wand = wPos[0];
        wPos.RemoveAt(0);
        return wand;
    }

    public static bool IsDepartmentExists(Departments dept)
    {
        return depts.ContainsKey(dept);
    }

    public static Transform GetInteractable(Departments dept)
    {
        if (dept == Departments.Start) return MathRand.Pick(instance.startPos);
        if (!IsDepartmentExists(dept)) return null;
        if (!intr.ContainsKey(dept)) intr.Add(dept, new List<Transform>());

        if(intr[dept].Count == 0) intr[dept].AddRange(MathRand.ChooseSetDependent(depts[dept].interactablePosition, depts[dept].interactablePosition.Length));
        
        var interact = intr[dept][0];
        intr[dept].RemoveAt(0);
        return interact;
    }

    public static Transform GetLookDirection(Departments dept)
    {
        if (!IsDepartmentExists(dept)) return null;
        return depts[dept].lookDirection;
    }

    public static GameObject GetDeptObject(Departments dept)
    {
        if (dept == Departments.Start) return null;
        return depts[dept].gameObject;
    }

    public static void CommitTransaction(Customer cust, GameType game)
    {
        if(Logistics.GetStock(game) > 0)
        {
            //Success
            cust.CommitTransaction(true);
            float revenue = MarketManager.GetSalePrice(game);
            Cash += revenue;
            CurrentXP += revenue;
            EndDayManager.AddRevenue(game, revenue);
            Logistics.ExpendGame(game);
        }
        else
        {
            //Failure
            cust.CommitTransaction(false);
        }
        
    }

    public static void CustomerLeave(GameObject custObj)
    {
        instance.DeselectAll();
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