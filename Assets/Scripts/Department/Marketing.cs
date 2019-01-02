using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marketing : DepartmentBase {

	// Use this for initialization
	protected override void Start () {
        base.Start();
        departmentName = "Marketing";
        GameManager.RegisterDepartment(Departments.Marketing, this);
    }

}
