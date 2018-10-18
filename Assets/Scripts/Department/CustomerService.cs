using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomerService : DepartmentBase {

    protected override void Start()
    {
        base.Start();
        GameManager.RegisterDepartment(Departments.CustService, this);
    }

}
