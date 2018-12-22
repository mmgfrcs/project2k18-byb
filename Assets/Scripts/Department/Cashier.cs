using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cashier : DepartmentBase {

    [Header("Upgrades"), SerializeField]
    private float[] baseServeSpeedUpgrade = new float[] { 4, 3.6f, 3.3f, 3f };
    [SerializeField]
    private float[] happinessGainUpgrade = new float[] { 7, 8, 9, 10 };
    [SerializeField]
    private List<Transform> interactablePosUpgrade;
    [SerializeField]
    private int[] maxStaffUpgrade = new int[] { 2, 4, 7, 10 };

    public float BaseServeSpeed { get { return baseServeSpeedUpgrade[UpgradeLevel - 1]; } }
    public float ServeSpeed { get { return BaseServeSpeed / WorkSpeed; } }

    protected override void Start()
    {
        base.Start();
        departmentName = "Cashier";
        UpdateSalary();
        GameManager.RegisterDepartment(Departments.Cashier, this);
    }

    internal void UpdateSalary()
    {
        UpdateSalary(Departments.Cashier);
    }

    public override void OnUpgrade()
    {
        base.OnUpgrade();
        interactablePosition.Add(interactablePosUpgrade[0]);
        interactablePosUpgrade.RemoveAt(0);
        MaximumStaff = maxStaffUpgrade[UpgradeLevel - 1];
    }
    // Update is called once per frame

}
