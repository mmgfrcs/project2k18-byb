using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forecaster : DepartmentBase {

    [Space]
    public float initialUncertainty = 50;

    internal float ForecastMinFactor { get; private set; }
    internal float ForecastMaxFactor { get; private set; }

    protected override void Start()
    {
        base.Start();
        departmentName = "Forecaster";
        //overtimeEffect = "Improves Next day Forecast's accuracy";
        ForecastMaxFactor = initialUncertainty / 100 + 1;
        ForecastMinFactor = 1 - initialUncertainty / 100;
        UpdateSalary();
        GameManager.RegisterDepartment(Departments.Forecaster, this);
    }

    internal void UpdateSalary()
    {
        UpdateSalary(Departments.Forecaster);
    }
}
