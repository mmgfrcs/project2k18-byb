using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricityOveruseEvent : GameEventSystems.Event {

    public override void Run()
    {
        Debug.Log("Run ElectricityOveruse");
        float cost = Random.Range(10, 50) * 10;
        GameManager.AdjustCash(cost);
        List<DepartmentBase> deptList = GameManager.GetAllDeptScripts();
        foreach (var dept in deptList) dept.AdjustTrust(-10);
    }

    public override void EndRun()
    {
        toEnd = true;
    }
}
