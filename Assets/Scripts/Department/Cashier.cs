using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cashier : DepartmentBase {

    // Use this for initialization
    public float salePrice = 8;

    protected override void Start()
    {
        base.Start();
        GameManager.RegisterDepartment(GameManager.Departments.Cashier, this);
    }

    // Update is called once per frame

}
