using System;
using UnityEngine;
using System.Collections.Generic;

public class HealAction : BaseBlessAction
{
    [SerializeField] private int healAmount = 25;

    protected override void PerformBlessEffect()
    {
        targetUnit.Heal(healAmount);
    }

    protected override bool CanPerformBlessAction()
    {
        return true; // Heal her zaman kullanılabilir
    }

    public override string GetActionName() => "Heal";

    public override int GetActionPointsCost()
    {
        return 5;
    }
}
