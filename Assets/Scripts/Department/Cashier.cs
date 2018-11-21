using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cashier : DepartmentBase {

    public float BaseServeSpeed { get; internal set; } = 4;
    public float ServeSpeed { get { return BaseServeSpeed / WorkSpeed; } }

    protected override void Start()
    {
        base.Start();
        departmentName = "Cashier";
        GameManager.RegisterDepartment(Departments.Cashier, this);
        
    }

    // Update is called once per frame

}
