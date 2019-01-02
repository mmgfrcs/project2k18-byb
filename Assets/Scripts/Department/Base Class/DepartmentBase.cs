using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DepartmentBase : MonoBehaviour, ISelectable {
    
    public List<Transform> interactablePosition;
    public Transform lookDirection;
    
    protected float trustDrainRate = 0f;
    
    internal string departmentName;
    
    public int UpgradeLevel { get; protected set; } = 1;
    public float CurrentTrust { get; protected set; } = 60;
    public float BaseWorkSpeed { get; protected set; } = 1;
    public float WorkSpeedMod { get; protected set; }
    public float WorkSpeed {
        get
        {
            return BaseWorkSpeed + WorkSpeedMod;
        }
    }
    public virtual bool IsFunctional { get { return WorkSpeed > 0; } }

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

	}

    // Update is called once per frame
    protected virtual void Update () {
        
	}

    internal virtual void AdjustTrust(float mod)
    {
        CurrentTrust = Mathf.Clamp(CurrentTrust + mod, 0, 100);
    }
}
