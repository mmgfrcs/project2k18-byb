using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finance : DepartmentBase {

    protected override void Start()
    {
        base.Start();
        departmentName = "Finance";
        GameManager.RegisterDepartment(Departments.Finance, this);
    }

    internal float ProcessExpense(float expense)
    {
        return expense;
    }
}
