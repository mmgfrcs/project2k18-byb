using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GameManager : MonoBehaviour {

    public enum Departments
    {
        Start, Showcase, Cashier
    }

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

    public float Cash { get; private set; }

    static GameManager instance;
    static Dictionary<Departments, DepartmentBase> depts = new Dictionary<Departments, DepartmentBase>();
    List<GameObject> recurringCust = new List<GameObject>();
    TKRotationRecognizer rt = new TKRotationRecognizer();
    float t = 0;
    int cid = 1;

    private void Awake()
    {
        depts = new Dictionary<Departments, DepartmentBase>();
    }

    // Use this for initialization
    void Start () {
        Cash = startingMoney;
        if (instance == null) instance = this;
        else Destroy(this);
        
        rt.gestureRecognizedEvent += (i) => {
            cameraPivot.Rotate(Vector3.up, -rt.deltaRotation * rotateSensitivity, Space.World);
        };
        TouchKit.addGestureRecognizer(rt);
    }

    Vector3 startTouchDir;
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
            ResourceManager.GetStock(ResourceManager.GameType.None), "Edwin's", 
            ResourceManager.GetStock(ResourceManager.GameType.GameA), "Rius'", 
            ResourceManager.GetStock(ResourceManager.GameType.GameB), "Derrick's", 
            ResourceManager.GetStock(ResourceManager.GameType.GameC), "Virya's");

        //Input
#if UNITY_ANDROID
        var touch = Input.touches;
        if(touch.Length == 1)
        {
            //Pan
            Vector3 delta = new Vector3(touch[0].deltaPosition.x, 0, touch[0].deltaPosition.y);
            if(touch[0].phase == TouchPhase.Moved) cameraPivot.Translate(delta * -Time.deltaTime * panSensitivity);
            
        }


#endif
    }

    public static void RegisterDepartment(Departments dept, DepartmentBase deptScript)
    {
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

    public static Transform GetInteractable(Departments dept)
    {
        if (dept == Departments.Start) return MathRand.Pick(instance.startPos);
        if (!intr.ContainsKey(dept)) intr.Add(dept, new List<Transform>());

        if(intr[dept].Count == 0) intr[dept].AddRange(MathRand.ChooseSetDependent(depts[dept].interactablePosition, depts[dept].interactablePosition.Length));
        
        var interact = intr[dept][0];
        intr[dept].RemoveAt(0);
        return interact;
    }

    public static GameObject GetDeptObject(Departments dept)
    {
        if (dept == Departments.Start) return null;
        return depts[dept].gameObject;
    }

    public static void CommitTransaction(Customer cust, ResourceManager.GameType game)
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