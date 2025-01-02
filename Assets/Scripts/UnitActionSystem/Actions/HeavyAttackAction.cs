using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class HeavyAttackAction : BaseMeleeAction
{
    [SerializeField] private int actionPointCost = 2;
    [SerializeField] private int damageAmount = 60;
    [SerializeField] protected override float stoppingDistance { get; set; } = 1.5f;
    [SerializeField] protected override float attackRange { get; set; } = 2f;
    [SerializeField] protected override float hitForce { get; set; } = 10f;
    

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
} 