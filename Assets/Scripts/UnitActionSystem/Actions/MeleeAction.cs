using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class MeleeAction : BaseMeleeAction
{
    [SerializeField] private int actionPointCost = 1;
    [SerializeField] private int damageAmount = 40;
    [SerializeField] private float meleeAttackRange = 2f;
    [SerializeField] private float meleeStoppingDistance = 1f;
    [SerializeField] private float meleeHitForce = 10f;

    protected override void OnStartAttack()
    {
        animator.SetTrigger("Attack");
    }

    protected override int GetDamageAmount()
    {
        return damageAmount;
    }

    protected override float GetHitForce()
    {
        return meleeHitForce;
    }

    protected override float GetStoppingDistance()
    {
        return meleeStoppingDistance;
    }

    protected override float GetAttackRange()
    {
        return meleeAttackRange;
    }

    public override int GetActionPointsCost()
    {
        return actionPointCost;
    }

    public override string GetActionName()
    {
        return "Melee";
    }
} 