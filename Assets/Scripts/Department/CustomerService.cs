using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerService : DepartmentBase {

    public float BaseServeSpeed { get; internal set; } = 20;
    public float ServeSpeed { get { return BaseServeSpeed / WorkSpeed; } }

    protected override void Start()
    {
        base.Start();
        trustDrainRate = 0.01f;
        departmentName = "Customer Service";
        GameManager.RegisterDepartment(Departments.CustService, this);
    }

}
