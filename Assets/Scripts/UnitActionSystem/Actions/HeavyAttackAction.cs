using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class HeavyAttackAction : BaseMeleeAction
{
    [SerializeField] private int actionPointCost = 2;
    [SerializeField] private int damageAmount = 60;
    [SerializeField] private float stoppingDistance = 1.5f; // bu ayarlar inspectordan değil scriptten değiştirilebilir
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float hitForce = 10f;
    

    protected override void OnStartAttack()
    {
        animator.SetTrigger("HeavyAttack");
    }

    protected override int GetDamageAmount()
    {
        return damageAmount;
    }

    protected override float GetHitForce()
    {
        return hitForce;
    }

    protected override float GetStoppingDistance()
    {
        return stoppingDistance;
    }

    protected override float GetAttackRange()
    {
        return attackRange;
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