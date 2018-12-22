using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DepartmentBase : MonoBehaviour, ISelectable {
    
    public List<Transform> interactablePosition;
    public Transform lookDirection;
    public int maxStaff = 1, startingStaff = 1;
    public float salary, hireCost, managerHireCost;
    public float minimumStaffRatio = 0.2f;
    public float fullSpeedStaffRatio = 0.8f;
    
    protected float trustDrainRate = 0f;
    
    internal string departmentName, overtimeEffect;
    internal List<SpecialAbility> abilities = new List<SpecialAbility>();
    
    internal bool Overtime { get; set; } = false;
    public int UpgradeLevel { get; protected set; } = 1;
    public int CurrentStaff { get; protected set; } = 1;
    public int MaximumStaff { get; protected set; } = 1;
    public float CurrentTrust { get; protected set; } = 60;
    public float BaseWorkSpeed { get; protected set; } = 1;
    public float WorkSpeedMod { get; protected set; }
    public float WorkSpeed {
        get
        {
            float minStaff = Mathf.Ceil(MaximumStaff * minimumStaffRatio);
            float fullSpeedStaff = Mathf.Ceil(MaximumStaff * fullSpeedStaffRatio);
            if (CurrentStaff < minStaff) return 0;
            if (fullSpeedStaff == minStaff) return BaseWorkSpeed + WorkSpeedMod;

            float workSpeed = (BaseWorkSpeed + WorkSpeedMod) * Mathf.LerpUnclamped(0.75f, 1f, (CurrentStaff - minStaff) / (fullSpeedStaff - minStaff));
            if (CurrentTrust < 20) return workSpeed * ((CurrentTrust + 1) / 21);
            return workSpeed;
        }
    }
    public virtual bool IsFunctional { get { return WorkSpeed > 0; } }
    public float StaffHireCost { get { return CurrentTrust >= 20 ? hireCost : hireCost * 2; } }
    public float ManagerHireCost { get { return CurrentTrust >= 20 ? managerHireCost : managerHireCost * 2; } }

    public virtual void OnUpgrade()
    {
        UpgradeLevel++;
    }

    public virtual void Deselect()
    {
        GetComponent<Outline>().enabled = false;
    }

    public virtual void Select()
    {
        GetComponent<Outline>().enabled = true;
    }

    // Use this for initialization
    protected virtual void Start () {
        GameManager.OnNextDay += OnOvertime;
        CurrentStaff = startingStaff;
        MaximumStaff = maxStaff;
	}

    protected virtual void AddAbility(SpecialAbility ability)
    {
        if (abilities.Count >= 2) abilities.RemoveAt(0);
        abilities.Add(ability);
    }

    protected virtual void OnSpecialAbilityEffect()
    {
        trustDrainRate = 0;
        WorkSpeedMod = 0;
        foreach(var ability in abilities)
        {
            switch (ability.abilityId)
            {
                case 0:
                    {
                        trustDrainRate = 1f / 60;
                        break;
                    }
                case 1:
                    {
                        WorkSpeedMod = 0.5f;

                        break;
                    }
            }
        }
    }

    protected virtual void OnOvertime()
    {

    }

    // Update is called once per frame
    protected virtual void Update () {
        OnSpecialAbilityEffect();
	}

    internal virtual void AdjustTrust(float mod)
    {
        CurrentTrust = Mathf.Clamp(CurrentTrust + mod, 0, 100);
    }

    internal virtual void AddStaff()
    {
        CurrentStaff = Mathf.Min(CurrentStaff + 1, MaximumStaff);
    }

    internal virtual void RemoveStaff()
    {
        CurrentStaff = Mathf.Max(CurrentStaff - 1, 0);
    }

    protected virtual void UpdateSalary(Departments dept)
    {
        float mod = GameManager.IsDepartmentExists(Departments.Finance) ? (GameManager.GetDeptScript(Departments.Finance) as Finance).ProcessExpense(salary * CurrentStaff) : 0;
        EndDayManager.AddExpense(ExpenseType.Salary, dept, salary * CurrentStaff, salary * CurrentStaff - mod);
    }
}
