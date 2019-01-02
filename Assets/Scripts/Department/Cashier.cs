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

    public float BaseServeSpeed { get { return baseServeSpeedUpgrade[UpgradeLevel - 1]; } }
    public float ServeSpeed { get { return BaseServeSpeed / WorkSpeed; } }

    protected override void Start()
    {
        base.Start();
        departmentName = "Cashier";
        GameManager.RegisterDepartment(Departments.Cashier, this);
    }

    public override void OnUpgrade()
    {
        base.OnUpgrade();
        interactablePosition.Add(interactablePosUpgrade[0]);
        interactablePosUpgrade.RemoveAt(0);
    }
    // Update is called once per frame

}
