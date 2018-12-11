using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackoutEvent : GameEventSystems.Event
{
    float visitMod = 0;
    public override void Run()
    {
        visitMod = GameManager.VisitChance * -0.8f;
        GameManager.SetVisitChanceMod(visitMod, true);
    }

    public override void EndRun()
    {
        GameManager.SetVisitChanceMod(-visitMod, true);
        toEnd = true;
    }
}
