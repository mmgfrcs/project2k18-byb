using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Customer : MonoBehaviour, ISelectable {

    public float startingHappiness = 60;
    //public float baseVisitChance = 0.8f;
    public float[] actionWeight = new float[] { 8, 2, 0 };

    [Header("Internal")]
    public float lookAngleBuffer = 0.25f;
    public float lookTime = 4, wanderTime = 10;
    public Material outlineMaterial;

    public float Happiness { get; private set; }
    public GameType GameDemand { get; private set; }

    internal string custName;
    internal float CurrentProgressTime { get; private set; }
    internal float MaxProgressTime
    {
        get
        {
            if (actionId == 0)
            {
                if (step == 0) return lookTime;
                else
                {
                    Cashier cashier = GameManager.GetDeptScript(Departments.Cashier) as Cashier;
                    return cashier.ServeSpeed;
                }
            }
            else if (actionId == 1) return wanderTime;
            else if (actionId == 2)
            {
                CustomerService customer = GameManager.GetDeptScript(Departments.CustService) as CustomerService;
                return customer.ServeSpeed;
            }
            else return 0;
        }
    }
    internal string CurrentActionName { get { return actions[actionId]; } }
    internal int Visits { get; private set; }

    internal float TotalSpendings { get; private set; }
    internal int CustomerID { get; private set; } = -1;
    
    bool finished = false, isInActivity = false;
    NavMeshAgent agent;
    readonly Material origMat;
    int actionId = -1, step = 0;
    Vector3 startPos;
    float origSpd;
    private readonly Dictionary<int, string> actions = new Dictionary<int, string>()
    {
        {-1, "Leaving" }, {0, "Shopping" }, {1, "Looking Around" }, {2, "Complaining" }
    };
    
    public void CommitTransaction(bool success, float amt = 5)
    {
        finished = true;
        Happiness += amt * (success ? 1 : -1);
        Happiness = Mathf.Clamp(Happiness, 0, 100);
    }

	// Use this for initialization
	void Start () {

        CustomerID = GameManager.RequestID();
        agent = GetComponent<NavMeshAgent>();
        origSpd = agent.speed;
        Happiness = startingHappiness;
        //What will this character do?
        ChooseAction();
	}

    public void ChooseAction()
    {
        Visits++;
        agent.speed = origSpd;
        GetComponent<MeshRenderer>().material.color = Color.white;
        finished = false;
        startPos = transform.position;

        float[] weight = new float[] {
            GameManager.IsDepartmentFunctional(Departments.Cashier) ? actionWeight[0] : 0,
            GameManager.IsDepartmentFunctional(Departments.CustService) ? actionWeight[1] : 0,
            actionWeight[2]
        };

        if (MathRand.WeightedPick(weight) == 0)
        {
            GameDemand = (GameType)MarketManager.GetDemands();
            StartCoroutine(GoShopping());
            //Shopping
        }
        else if (MathRand.WeightedPick(weight) == 1)
        {
            StartCoroutine(GoWander());
            //Wander
        }
        else if (MathRand.WeightedPick(weight) == 2 && GameManager.IsDepartmentFunctional(Departments.CustService))
        {
            
            StartCoroutine(GoComplain()); //Complain
        }
        else ChooseAction();
    }
	
	// Update is called once per frame
	void Update () {
        if (!finished && actionId == 0) Happiness -= Time.deltaTime / 6;
        //var dest = new Vector3(agent.destination.x, transform.position.y, agent.destination.z);

        if (actionId == 1)
        {
            CurrentProgressTime += Time.deltaTime;
            if (CurrentProgressTime >= wanderTime)
            {
                StopAllCoroutines();
                StartCoroutine(LeaveBusiness());
                CurrentProgressTime = 0;

            }
        }
        else if(actionId == 0) {
            if (isInActivity) CurrentProgressTime += Time.deltaTime;
            else CurrentProgressTime = 0;
        }
        else if (actionId == 2)
        {
            if (isInActivity) CurrentProgressTime += Time.deltaTime;
            else CurrentProgressTime = 0;
        }
        if (CurrentProgressTime > MaxProgressTime) CurrentProgressTime = MaxProgressTime;
	}

    public void Deselect()
    {
        GetComponent<Outline>().enabled = false;
    }

    public void Select()
    {
        GetComponent<Outline>().enabled = true;
    }

    IEnumerator GoShopping()
    {
        actionId = 0;
        step = 0;
        //Move to interact point of the Showcase. TODO: Choose interact point
        agent.SetDestination(GameManager.GetInteractable(Departments.Showcase).position);
        yield return new WaitUntil(() => {
            return CheckDistance(agent.destination);
        });

        //Look at the parent object
        yield return new WaitUntil(() => {
            return RotateTowards(GameManager.GetDeptObject(Departments.Showcase).transform.position) < lookAngleBuffer;
        });

        SetObstruction(true);
        isInActivity = true;
        //Wait to visualize look. Animate
        yield return new WaitForSeconds(lookTime);

        isInActivity = false;
        SetObstruction(false);
        //Check for availability
        if (Logistics.GetStock(GameDemand) <= 0)
        {
            CommitTransaction(false);
            StartCoroutine(LeaveBusiness());
            yield break;
        }

        step++;
        //Move character. TODO: Choose interact point
        Cashier cashier = GameManager.GetDeptScript(Departments.Cashier) as Cashier;
        agent.SetDestination(GameManager.GetInteractable(Departments.Cashier).position);
        yield return new WaitUntil(() => {
            return CheckDistance(agent.destination);
        });

        //Look at the parent object
        yield return new WaitUntil(() => {
            return RotateTowards(GameManager.GetDeptObject(Departments.Cashier).transform.position) < lookAngleBuffer;
        });

        SetObstruction(true);
        isInActivity = true;
        //Wait to visualize look. Animate
        yield return new WaitForSeconds(cashier.ServeSpeed);
        
        //Do transaction, then wait as visualization
        TotalSpendings += GameManager.CommitTransaction(this, GameDemand);
        yield return new WaitForSeconds(1.5f);

        isInActivity = false;

        SetObstruction(false);
        //Move out from the shop.
        StartCoroutine(LeaveBusiness());
    }

    IEnumerator GoWander()
    {
        actionId = 1;
        agent.speed *= 0.75f;

        while (CurrentProgressTime < wanderTime)
        {
            //try
            //{
                //Move to wander point
                agent.SetDestination(GameManager.GetWander().position);
            //}
            //catch (System.Exception)
            //{
            //    Visits--;
            //    StartCoroutine(LeaveBusiness());
            //}

            yield return new WaitUntil(() =>
            {
                return CheckDistance(agent.destination) || CurrentProgressTime >= wanderTime;
            });

            //Look at random direction
            Vector3 dir = new Vector3(transform.position.x + Random.Range(-10, 10), transform.position.y, transform.position.z + Random.Range(-10, 10));
            yield return new WaitUntil(() =>
            {
                return RotateTowards(dir) < lookAngleBuffer || CurrentProgressTime >= wanderTime;
            });
            SetObstruction(true);
            //Wait arbitrarily
            yield return new WaitForSeconds(Random.Range(1f, lookTime));
            SetObstruction(false);

        }
    }

    IEnumerator GoComplain()
    {
        actionId = 2;

        //Go to CS
        agent.SetDestination(GameManager.GetInteractable(Departments.CustService).position);
        yield return new WaitUntil(() => {
            return CheckDistance(agent.destination);
        });

        //Look at the parent object
        yield return new WaitUntil(() => {
            return RotateTowards(GameManager.GetDeptObject(Departments.CustService).transform.position) < lookAngleBuffer;
        });

        CustomerService cust = GameManager.GetDeptScript(Departments.CustService) as CustomerService;

        SetObstruction(true);
        isInActivity = true;
        //Wait to visualize look. Animate
        yield return new WaitForSeconds(cust.ServeSpeed);
        isInActivity = false;

        CommitTransaction(true, 16);
        SetObstruction(false);
        StartCoroutine(LeaveBusiness());

        //yield return null;
    }

    IEnumerator LeaveBusiness()
    {
        actionId = -1;
        //Move out from the shop.
        agent.avoidancePriority = 40;
        agent.SetDestination(startPos);
        yield return new WaitUntil(() => {
            return CheckDistance(startPos);
        });
        yield return new WaitForSeconds(0.25f);

        //DEBUG ONLY
        GameManager.CustomerLeave(gameObject);
        //Destroy(gameObject);
    }

    void SetObstruction(bool obs)
    {
        if (obs)
        {
            agent.avoidancePriority = 25;
            //agent.enabled = false;
            //this.obs.enabled = true;
        }
        else
        {
            agent.avoidancePriority = 50;
            //this.obs.enabled = false;
            //agent.enabled = true;
        }
    }

    float RotateTowards(Vector3 dest)
    {
        dest.y = transform.position.y;
        var dir = dest - transform.position;
        var rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 4);
        return Quaternion.Angle(transform.rotation, rot);
    }

    bool CheckDistance(Vector3 dest)
    {
        dest = new Vector3(dest.x, transform.position.y, dest.z);
        return Vector3.Distance(transform.position, dest) < 1f;
    }
}
