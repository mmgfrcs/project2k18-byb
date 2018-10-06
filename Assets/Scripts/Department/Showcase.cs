using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Showcase : DepartmentBase {

    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        GameManager.RegisterDepartment(GameManager.Departments.Showcase, this);
    }

    // Update is called once per frame

}
