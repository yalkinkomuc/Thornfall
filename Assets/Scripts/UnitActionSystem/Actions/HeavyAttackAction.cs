using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class HeavyAttackAction : BaseMeleeAction
{
    [Header("Heavy Attack Settings")]
    [SerializeField] private int actionPointCost = 2;
    [SerializeField] private int damageAmount = 60;
    
    [Header("Override Base Settings")]
    [SerializeField] private float attackRangeOverride = 2f;
    [SerializeField] private float stoppingDistanceOverride = 1.5f;
    [SerializeField] private float hitForceOverride = 25f;

    protected override float GetAttackRange() => attackRangeOverride;
    protected override float GetStoppingDistance() => stoppingDistanceOverride;
    protected override float GetHitForce() => hitForceOverride;

    protected override void OnStartAttack()
    {
        animator.SetTrigger("HeavyAttack");
    }

    protected override int GetDamageAmount()
    {
        return damageAmount;
    }

    public override int GetActionPointsCost()
    {
        return actionPointCost;
    }

    public override string GetActionName()
    {
        return "Heavy Attack";
    }

    protected override StatusEffect GetStatusEffect(Unit target)
    {
        return null;// Heavy attack kanama efekti uygular
    }
} 