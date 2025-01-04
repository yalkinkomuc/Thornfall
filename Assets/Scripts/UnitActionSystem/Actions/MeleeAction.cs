using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class MeleeAction : BaseMeleeAction
{
    [Header("Melee Settings")]
    [SerializeField] private int actionPointCost = 1;
    [SerializeField] private int damageAmount = 40;
    
    [Header("Override Base Settings")]
    [SerializeField] private float attackRangeOverride = 2f;
    [SerializeField] private float stoppingDistanceOverride = 1f;
    [SerializeField] private float hitForceOverride = 10f;

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
        return hitForceOverride;
    }

    protected override float GetStoppingDistance()
    {
        return stoppingDistanceOverride;
    }

    protected override float GetAttackRange()
    {
        return attackRangeOverride;
    }

    public override int GetActionPointsCost()
    {
        return actionPointCost;
    }

    public override string GetActionName()
    {
        return "Melee";
    }

    protected override StatusEffect GetStatusEffect(Unit target)
    {
        return null;
    }
} 