
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Customer : MonoBehaviour, ISelectable {

    public float startingHappiness = 60;
    public float baseVisitChance = 0.8f;
    public float[] actionWeight = new float[] { 8, 2, 0 };
    public Text debugText;

    [Header("Internal")]
    public float lookAngleBuffer = 0.25f;
    public float lookTime = 4, complainTime = 20, wanderTime = 10;
    public Material outlineMaterial;

    public float Happiness { get; private set; }
    public GameType GameDemand { get; private set; }

    internal string custName;
    internal float CurrentProgressTime { get { return t; } }
    internal float MaxProgressTime
    {
        get
        {
            if (actionId == 0) return lookTime;
            else if (actionId == 1) return wanderTime;
            else if (actionId == 2) return complainTime;
            else return 0;
        }
    }
    internal string CurrentActionName { get { return actions[actionId]; } }
    internal int CustomerID { get { return cId; } }
    

    float prog = 0;
    bool finished = false, isInActivity = false;
    NavMeshAgent agent;
    NavMeshObstacle obs;
    Material origMat;
    int actionId = -1, cId = -1;
    float t;
    Vector3 startPos;
    float origSpd;
    Dictionary<int, string> actions = new Dictionary<int, string>()
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

        cId = GameManager.RequestID();
        agent = GetComponent<NavMeshAgent>();
        obs = GetComponent<NavMeshObstacle>();
        origSpd = agent.speed;
        Happiness = startingHappiness;
        //What will this character do?
        ChooseAction();
	}

    public void ChooseAction()
    {
        agent.speed = origSpd;
        GetComponent<MeshRenderer>().material.color = Color.white;
        finished = false;
        startPos = transform.position;
        if (MathRand.WeightedPick(actionWeight) == 0)
        {
            GameDemand = (GameType)MarketManager.GetDemands();
            StartCoroutine(GoShopping());
            //Shopping
        }
        else if (MathRand.WeightedPick(actionWeight) == 1)
        {
            StartCoroutine(GoWander());
            //Wander
        }
        else if (MathRand.WeightedPick(actionWeight) == 2 && GameManager.IsDepartmentExists(Departments.CustService))
        {
            
            StartCoroutine(GoComplain()); //Complain
        }
        else ChooseAction();
    }
	
	// Update is called once per frame
	void Update () {
        if (!finished && actionId == 0) Happiness -= Time.deltaTime / 6;
        string action = actionId > 0 ? "Wander" : actionId == 0 ? "Shop" : "Leaving";
        //var dest = new Vector3(agent.destination.x, transform.position.y, agent.destination.z);
        debugText.text = string.Format("({3}) {0:N1}%\n{1} {2}", Happiness, action, actionId == 0 ? MarketManager.GetGameNames(GameDemand) : "", cId);

        if (actionId == 1)
        {
            t += Time.deltaTime;
            if (t >= wanderTime)
            {
                StopAllCoroutines();
                StartCoroutine(LeaveBusiness());
                t = 0;

            }
        } else if(actionId == 0) {
            if (isInActivity) t += Time.deltaTime;
            else t = 0;
            if (t > lookTime) t = lookTime;
            
        } else if (actionId == 2)
        {
            if (isInActivity) t += Time.deltaTime;
            else t = 0;
            if (t > complainTime) t = complainTime;
        }
	}

    public void Select()
    {

    }

    public void Deselect()
    {

    }

    IEnumerator GoShopping()
    {
        actionId = 0;

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

        //Move character. TODO: Choose interact point
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
        yield return new WaitForSeconds(lookTime);

        isInActivity = false;
        //Do transaction, then wait as visualization
        GameManager.CommitTransaction(this, GameDemand);
        yield return new WaitForSeconds(1.5f);

        SetObstruction(false);
        //Move out from the shop.
        StartCoroutine(LeaveBusiness());
    }

    IEnumerator GoWander()
    {
        actionId = 1;
        agent.speed *= 0.75f;

        while (t < wanderTime)
        {
            //Move to wander point
            agent.SetDestination(GameManager.GetWander().position);
            yield return new WaitUntil(() =>
            {
                return CheckDistance(agent.destination) || t >= wanderTime;
            });

            //Look at random direction
            Vector3 dir = new Vector3(transform.position.x + Random.Range(-10, 10), transform.position.y, transform.position.z + Random.Range(-10, 10));
            yield return new WaitUntil(() =>
            {
                return RotateTowards(dir) < lookAngleBuffer || t >= wanderTime;
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

        SetObstruction(true);
        isInActivity = true;
        //Wait to visualize look. Animate
        yield return new WaitForSeconds(complainTime);
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
