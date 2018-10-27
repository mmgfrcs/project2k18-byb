using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DepartmentBase : MonoBehaviour {

    public string departmentName;
    public Transform[] interactablePosition;
    public Transform lookDirection;

    protected int currStaff;

    internal int CurrentStaff { get { return currStaff; } }
    internal bool IsFunctional { get { return currStaff > 0; } }

    // Use this for initialization
    protected virtual void Start () {
		
	}

    // Update is called once per frame
    protected virtual void Update () {
		
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
