﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Marketing : DepartmentBase {

	// Use this for initialization
	protected override void Start () {
        GameManager.RegisterDepartment(Departments.Marketing, this);
    }

    // Update is called once per frame
    protected override void Update()
    {

    }
    
}