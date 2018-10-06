using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Customer : MonoBehaviour {

    public float startingHappiness = 60;
    public float baseVisitChance = 0.8f;
    public float[] actionWeight = new float[] { 8, 2 };
    public Text debugText;

    [Header("Internal")]
    public float lookAngleBuffer = 0.25f;
    public float lookSpeed = 4;

    public float Happiness { get; private set; }
    public ResourceManager.GameType GameDemand { get; private set; }

    float liveTime = 10;
    float lookTime = 4;
    bool finished = false;
    NavMeshAgent agent;
    NavMeshObstacle obs;

    int actionId = -1, cId = -1;
    float t;

    Vector3 startPos;
    float origSpd;

    public void CommitTransaction(bool success)
    {
        finished = true;
        Happiness += 9 * (success ? 1 : -1);
        Happiness = Mathf.Clamp(Happiness, 0, 100);
        GetComponent<MeshRenderer>().material.color = Color.green;
    }

	// Use this for initialization
	void Start () {

        cId = GameManager.RequestID();
        agent = GetComponent<NavMeshAgent>();
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
            GameDemand = (ResourceManager.GameType)MarketManager.GetDemands();
            StartCoroutine(GoShopping());
            //Shopping
        }
        else
        {
            StartCoroutine(GoWander());
            //Wander
        }
    }
	
	// Update is called once per frame
	void Update () {
        if (!finished && actionId == 0) Happiness -= Time.deltaTime / 4;
        string action = actionId > 0 ? "Wander" : actionId == 0 ? "Shop" : "Leaving";
        var dest = new Vector3(agent.destination.x, transform.position.y, agent.destination.z);
        debugText.text = string.Format("({3}) {0:N1}%\n{1} {2}", Happiness, action, actionId == 0 ? MarketManager.GetGameNames(GameDemand) : "", cId);

        if (actionId == 1)
        {
            t += Time.deltaTime;
            if(t>= liveTime)
            {
                StopAllCoroutines();
                StartCoroutine(LeaveBusiness());
                t = 0;

            }
        }
        /*
        if (agent.velocity.magnitude > 0.01f)
        {
            obs.enabled = false;
            agent.enabled = true;
        }
        else
        {
            agent.enabled = false;
            obs.enabled = true;
        }*/
	}

    IEnumerator GoShopping()
    {
        actionId = 0;

        //Move to interact point of the Showcase. TODO: Choose interact point
        agent.SetDestination(GameManager.GetInteractable(GameManager.Departments.Showcase).position);
        yield return new WaitUntil(() => {
            return CheckDistance(agent.destination);
        });

        //Look at the parent object
        yield return new WaitUntil(() => {
            return RotateTowards(GameManager.GetDeptObject(GameManager.Departments.Showcase).transform.position) < 0.25f;
        });

        //Wait to visualize look. Animate
        yield return new WaitForSeconds(lookTime);
        GetComponent<MeshRenderer>().material.color = Color.cyan;
        //Check for availability
        if(ResourceManager.GetStock(ResourceManager.GameType.None) <= 0)
        {
            CommitTransaction(false);
            StartCoroutine(LeaveBusiness());
            yield break;
        }

        //Move character. TODO: Choose interact point
        agent.SetDestination(GameManager.GetInteractable(GameManager.Departments.Cashier).position);
        yield return new WaitUntil(() => {
            return CheckDistance(agent.destination);
        });

        //Look at the parent object
        yield return new WaitUntil(() => {
            return RotateTowards(GameManager.GetDeptObject(GameManager.Departments.Cashier).transform.position) < 0.25f;
        });
        

        //Wait to visualize look. Animate
        yield return new WaitForSeconds(lookTime);

        //Do transaction, then wait as visualization
        GameManager.CommitTransaction(this, GameDemand);
        yield return new WaitForSeconds(1.5f);

        //Move out from the shop.
        StartCoroutine(LeaveBusiness());
    }

    IEnumerator GoWander()
    {
        actionId = 1;
        agent.speed *= 0.75f;

        while (t < liveTime)
        {
            //Move to wander point
            agent.SetDestination(GameManager.GetWander().position);
            yield return new WaitUntil(() =>
            {
                return CheckDistance(agent.destination) || t >= liveTime;
            });

            //Look at random direction
            Vector3 dir = new Vector3(transform.position.x + Random.Range(-10, 10), transform.position.y, transform.position.z + Random.Range(-10, 10));
            yield return new WaitUntil(() =>
            {
                return RotateTowards(dir) < 0.25f || t >= liveTime;
            });

            //Wait arbitrarily
            yield return new WaitForSeconds(Random.Range(1f, lookTime));
        }
    }

    IEnumerator LeaveBusiness()
    {
        actionId = -1;
        //Move out from the shop.
        agent.SetDestination(startPos);
        yield return new WaitUntil(() => {
            return CheckDistance(startPos);
        });
        yield return new WaitForSeconds(0.25f);

        //DEBUG ONLY
        GameManager.CustomerLeave(gameObject);
        //Destroy(gameObject);
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
