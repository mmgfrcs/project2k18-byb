using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Departments
{
    Start, Showcase, Cashier, CustService
}

public class GameManager : MonoBehaviour {

    [Header("Mechanics")]
    public float startingMoney;
    public float baseTime = 6f;

    [Header("Objects")]
    public Transform[] startPos;
    public Transform[] wanderPos;
    public GameObject[] customerObjects;
    public Transform cameraPivot;

    [Header("Settings")]
    public float panSensitivity = 0.3f;
    public float rotateSensitivity = 1f;

    [Header("UI")]
    public Text moneyText;
    public Text stockText;
    public RectTransform infoPanel;
    public InfoPanel infoPanelContents;

    public float Cash { get; private set; }

    static GameManager instance;
    static Dictionary<Departments, DepartmentBase> depts = new Dictionary<Departments, DepartmentBase>();
    List<GameObject> recurringCust = new List<GameObject>();
    Animator infoPanelAnim;
    TKRotationRecognizer rt = new TKRotationRecognizer();
    TKPinchRecognizer pr = new TKPinchRecognizer();

    Customer selectedCustomer;
    DepartmentBase selectedDept;

    float t = 0;
    int cid = 1;

    private void Awake()
    {
        depts = new Dictionary<Departments, DepartmentBase>();
    }

    // Use this for initialization
    void Start () {
        if (instance == null) instance = this;
        else Destroy(this);

        Cash = startingMoney;
        infoPanelAnim = infoPanel.GetComponent<Animator>();

        rt.gestureRecognizedEvent += (i) => {
            cameraPivot.Rotate(Vector3.up, -rt.deltaRotation * rotateSensitivity, Space.World);
        };

        pr.gestureRecognizedEvent += (i) =>
        {
            Camera.main.transform.Translate(Vector3.forward * 2 * pr.deltaScale);
        };
        TouchKit.addGestureRecognizer(rt);
        TouchKit.addGestureRecognizer(pr);
    }
    
	// Update is called once per frame
	void Update () {
        t += Time.deltaTime;
        if (t >= baseTime)
        {
            t = 0;
            Transform start = MathRand.Pick(startPos);
            int choice = MathRand.WeightedPick(new float[] { recurringCust.Count, 1 });

            if (choice == 1) Instantiate(MathRand.Pick(customerObjects), start.position, start.rotation); //If it's a spawnable prefab we instantiate it
            else CustomerReturn(MathRand.Pick(recurringCust)); //else we return it
        }

        moneyText.text = string.Format("${0:N1}", Cash);

        stockText.text = string.Format("{0:N0} {1}\n{2:N0} {3}\n{4:N0} {5}\n{6:N0} {7}", 
            ResourceManager.GetStock(GameType.None), MarketManager.GetGameNames(GameType.None), 
            ResourceManager.GetStock(GameType.GameA), MarketManager.GetGameNames(GameType.GameA), 
            ResourceManager.GetStock(GameType.GameB), MarketManager.GetGameNames(GameType.GameB), 
            ResourceManager.GetStock(GameType.GameC), MarketManager.GetGameNames(GameType.GameC));

        //Input (touch)
        var touch = Input.touches;
        if (touch.Length == 1 && touch[0].deltaPosition.magnitude > 0.2f)
        {
            Vector3 delta = new Vector3(touch[0].deltaPosition.x, 0, touch[0].deltaPosition.y);
            cameraPivot.Translate(delta * -Time.deltaTime * panSensitivity);
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Customer cust = hit.collider.GetComponent<Customer>();
                if (cust != null)
                {
                    if (selectedCustomer != null) selectedCustomer.Deselect();
                    else SetPanelCloseState(true);
                    selectedCustomer = cust;
                    selectedCustomer.Select();

                }
                else
                {
                    if (selectedCustomer != null) DeselectAll();
                }
            }
        }


        if (selectedCustomer != null)
        {
            infoPanelContents.NameText.text = "Customer " + selectedCustomer.CustomerID;
            infoPanelContents.Text1Text.text = selectedCustomer.CurrentActionName + " " + (selectedCustomer.CurrentActionName == "Shopping" ? MarketManager.GetGameNames(selectedCustomer.GameDemand) : "");
            infoPanelContents.Bar1Slider.maxValue = 100;
            infoPanelContents.Bar1Slider.value = selectedCustomer.Happiness;
            infoPanelContents.Progress1Slider.maxValue = selectedCustomer.MaxProgressTime;
            infoPanelContents.Progress1Slider.value = selectedCustomer.CurrentProgressTime;
            
            //infoPanelContents.Bar1Image.sprite = Hearts
        }
    }
    
    public void DeselectAll()
    {
        SetPanelCloseState(false);
        selectedCustomer.Deselect();
        selectedCustomer = null;
    }

    public void SetPanelCloseState(bool open)
    {
        if((open && IsInfoPanelClosed()) || (!open && !IsInfoPanelClosed())) infoPanelAnim.Play(open ? "Open" : "Close");
    }

    bool IsInfoPanelClosed()
    {
        return infoPanelAnim.GetCurrentAnimatorStateInfo(0).IsName("Closed");
    }

    public static void RegisterDepartment(Departments dept, DepartmentBase deptScript)
    {
        print("Department Registered: " + dept);
        depts.Add(dept, deptScript);
    }

    public static void RegisterMap(Transform[] startPos, Transform[] wanderPos)
    {

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
        if(ResourceManager.GetStock(game) > 0)
        {
            //Success
            cust.CommitTransaction(true);
            instance.Cash += MarketManager.GetPrices(game);
            ResourceManager.ExpendGame(game);
        }
        else
        {
            //Failure
            cust.CommitTransaction(false);
        }
        
    }

    public static void CustomerLeave(GameObject custObj)
    {
        if (instance.selectedCustomer == custObj.GetComponent<Customer>())
        {
            instance.DeselectAll();
        }
        instance.recurringCust.Add(custObj);
        custObj.SetActive(false);
        custObj.transform.position = new Vector3(-1000, -1000, -1000);
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