using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cashier : DepartmentBase {
    

    protected override void Start()
    {
        base.Start();
        departmentName = "Cashier";
        maxStaff = 2;
        GameManager.RegisterDepartment(Departments.Cashier, this);
        
    }

    // Update is called once per frame

}
