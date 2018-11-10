using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerService : DepartmentBase {

    protected override void Start()
    {
        base.Start();
        trustDrainRate = 0.01f;
        departmentName = "Customer Service";
        maxStaff = 1;
        GameManager.RegisterDepartment(Departments.CustService, this);
    }

}
