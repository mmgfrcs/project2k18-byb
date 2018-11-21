using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResearchDevelopment : DepartmentBase {
    protected override void Start()
    {
        base.Start();
        departmentName = "R&D";
        GameManager.RegisterDepartment(Departments.Research, this);
    }
}
