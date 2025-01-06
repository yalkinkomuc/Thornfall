using UnityEngine;

public class HeavyAttack_Berserker : BaseMeleeAction
{
    [Header("Berserker Settings")]
    [SerializeField] private int actionPointCost = 2;
    [SerializeField] private int damageAmount = 70;
    [SerializeField] private int bleedDamageAmount = 10;
    [SerializeField] private int bleedDurationPerTurn = 1;
    
    [Header("Override Base Settings")]
    [SerializeField] private float attackRangeOverride = 2.5f;
    [SerializeField] private float stoppingDistanceOverride = 1.5f;
    [SerializeField] private float hitForceOverride = 30f;

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
        return new PoisonEffect(target, bleedDurationPerTurn, bleedDamageAmount); // Berserker heavy attack için yanma efekti
    }
}
