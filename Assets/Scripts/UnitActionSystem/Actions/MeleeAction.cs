using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class MeleeAction : BaseMeleeAction
{
    [SerializeField] private int actionPointCost = 1;
    [SerializeField] private int damageAmount = 40;
    [SerializeField] protected override float stoppingDistance { get; set; } = 1.5f;
    [SerializeField] protected override float attackRange { get; set; } = 2f;
    [SerializeField] protected override float hitForce { get; set; } = 10f;

    protected override void OnStartAttack()
    {
        animator.SetTrigger("Attack");
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
        return "Melee";
    }
} 