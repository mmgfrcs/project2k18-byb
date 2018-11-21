using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepartmentBase : MonoBehaviour, ISelectable {
    
    public Transform[] interactablePosition;
    public Transform lookDirection;
    public int maxStaff = 1;
    public float salary;
    public float minimumStaffRatio = 0.2f;
    public float fullSpeedStaffRatio = 0.8f;
    
    protected float trustDrainRate = 0.02f;
    
    internal string departmentName, overtimeEffect;

    internal bool Overtime { get; set; } = false;
    public int CurrentStaff { get; protected set; } = 1;
    public int MaximumStaff { get; protected set; } = 1;
    public float CurrentTrust { get; protected set; } = 60;
    public float BaseWorkSpeed { get; protected set; } = 1;
    public float WorkSpeed {
        get
        {
            float minStaff = Mathf.Ceil(MaximumStaff * minimumStaffRatio);
            float fullSpeedStaff = Mathf.Ceil(MaximumStaff * fullSpeedStaffRatio);
            if (CurrentStaff < minStaff) return 0;
            if (fullSpeedStaff == minStaff) return BaseWorkSpeed;
            return BaseWorkSpeed * Mathf.LerpUnclamped(0.75f, 1f, (CurrentStaff - minStaff) / (fullSpeedStaff - minStaff));
        }
    }
    internal bool IsFunctional { get { return WorkSpeed > 0 && CurrentTrust >= 20; } }

    public virtual void Deselect()
    {
        
    }

    public virtual void Select()
    {
        
    }

    // Use this for initialization
    protected virtual void Start () {
        GameManager.OnNextDay += GameManager_OnNextDay;
        MaximumStaff = maxStaff;
	}

    protected virtual void GameManager_OnNextDay()
    {

    }

    // Update is called once per frame
    protected virtual void Update () {
        CurrentTrust -= Time.deltaTime * trustDrainRate;
	}

    internal virtual void AddStaff()
    {
        CurrentStaff = Mathf.Min(CurrentStaff + 1, MaximumStaff);
    }

    internal virtual void RemoveStaff()
    {
        CurrentStaff = Mathf.Max(CurrentStaff - 1, 0);
    }

    
}
