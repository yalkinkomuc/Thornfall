using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class BowRangeAction : BaseRangeAction
{
    [Header("Bow Range Settings")]
    [SerializeField] private int actionPointsCost = 2;
    [SerializeField] private int damageAmount = 10;

    public override string GetActionName() => "Range Attack";

    public override int GetActionPointsCost()
    {
        if (GetValidTarget(GetRange()) == null)
        {
            return 0;
        }
        return actionPointsCost;
    }

    public override int GetDamageAmount()
    {
        return damageAmount;
    }

    protected override ElementalSpells GetElementalSpell(Unit target)
    {
        return null;
    }

    protected override void Start()
    {
        base.Start();
        OnShootAnimStarted += BowRangeAction_OnShootAnimStarted;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnShootAnimStarted -= BowRangeAction_OnShootAnimStarted;
    }

    private void BowRangeAction_OnShootAnimStarted(object sender, EventArgs e)
    {
        // Burada gerekirse özel işlemler yapılabilir.
    }

    protected override void OnStartAttack()
    {
        animator.SetTrigger("Shoot");
    }
}
