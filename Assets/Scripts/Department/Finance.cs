using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Finance : DepartmentBase {

    public float[] expenseRatios = new float[] { 0.984f, 0.981f, 0.977f, 0.973f };

    internal int Level { get; private set; } = 1;

    protected override void Start()
    {
        base.Start();
        departmentName = "Finance";
        GameManager.RegisterDepartment(Departments.Finance, this);
    }

    internal float ProcessExpense(float expense, int staff)
    {
        float rat = Mathf.Pow(expenseRatios[Level], staff);
        return expense * rat;
    }

    public void UpgradeLevel()
    {
        if (Level < 4) Level++;
    }
}
