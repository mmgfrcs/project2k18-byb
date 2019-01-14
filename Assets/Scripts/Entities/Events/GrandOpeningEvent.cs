using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrandOpeningEvent : GameEventSystems.Event
{
    public override void EndRun()
    {
        
    }

    public override void Run()
    {
        List<DepartmentBase> deptList = GameManager.GetAllDeptScripts();
        foreach (var dept in deptList) dept.AdjustTrust(10);
    }
}
