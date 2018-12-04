using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marketing : DepartmentBase {

	// Use this for initialization
	protected override void Start () {
        base.Start();
        departmentName = "Marketing";
        //overtimeEffect = "Increase Ads Effect";
        UpdateSalary();
        GameManager.RegisterDepartment(Departments.Marketing, this);
    }

    internal void UpdateSalary()
    {
        UpdateSalary(Departments.Marketing);
    }

}
