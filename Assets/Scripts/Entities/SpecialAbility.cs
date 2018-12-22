using System;
using System.Collections.Generic;
using UnityEngine;

public class SpecialAbility : MonoBehaviour
{
    public int abilityId;
    public Departments department;
    public Sprite abilityIcon;
    [Space]
    public string abilityName;
    [TextArea]
    public string abilityDescription;
    public bool trainable = true;
    public string[] gainConditions, removeConditions;
    public float discoveryTimeHours, trainTimeHours;
    
    public bool AbilityDiscovered { get; private set; }
    
    public virtual bool IsAbilityAttachable(Departments dept)
    {
        if (department != Departments.Start && dept != department) return false;

        return true;
    }

    public void DiscoverAbility()
    {
        AbilityDiscovered = true;
    }
    
}

