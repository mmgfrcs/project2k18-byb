using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanResource : DepartmentBase {

    protected override void Start()
    {
        base.Start();
        departmentName = "HRD";
        UpdateSalary();
        GameManager.RegisterDepartment(Departments.HRD, this);
    }

    internal void UpdateSalary()
    {
        UpdateSalary(Departments.HRD);
    }
}
