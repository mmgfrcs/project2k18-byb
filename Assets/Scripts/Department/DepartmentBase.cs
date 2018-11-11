using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepartmentBase : MonoBehaviour, ISelectable {

    
    public Transform[] interactablePosition;
    public Transform lookDirection;
    
    protected float trustDrainRate = 0.02f;
    protected int currStaff, maxStaff;
    protected float trust;

    internal string departmentName;

    internal bool Overtime { get; set; }
    internal int CurrentStaff { get { return currStaff; } }
    internal int MaximumStaff { get { return maxStaff; } }
    internal float CurrentTrust { get { return trust; } }
    internal bool IsFunctional { get { return currStaff > 0 && trust >= 10; } }

    public virtual void Deselect()
    {
        
    }

    public virtual void Select()
    {
        
    }

    // Use this for initialization
    protected virtual void Start () {
        trust = 60;
        currStaff = 1;
        GameManager.OnNextDay += GameManager_OnNextDay;
	}

    protected virtual void GameManager_OnNextDay()
    {

    }

    // Update is called once per frame
    protected virtual void Update () {
        trust -= Time.deltaTime * trustDrainRate;
	}

    internal virtual void AddStaff()
    {
        currStaff++;
    }

    internal virtual void RemoveStaff()
    {
        currStaff = Mathf.Max(currStaff - 1, 0);
    }

    
}
