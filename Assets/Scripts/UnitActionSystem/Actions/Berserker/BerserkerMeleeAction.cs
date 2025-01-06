using UnityEngine;

public class BerserkerMeleeAction : BaseMeleeAction
{
    [Header("Melee Attack Settings")]
    [SerializeField] private int actionPointCost = 1;
    [SerializeField] private int damageAmount = 40;
   // [SerializeField] private int poisonDamageAmount = 5;
    //[SerializeField] private int poisonDurationPerTurn = 2;
    
    [Header("Override Base Settings")]
    [SerializeField] private float attackRangeOverride = 2f;
    [SerializeField] private float stoppingDistanceOverride = 1f;
    [SerializeField] private float hitForceOverride = 15f;

    protected override float GetAttackRange() => attackRangeOverride;
    protected override float GetStoppingDistance() => stoppingDistanceOverride;
    protected override float GetHitForce() => hitForceOverride;

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
        return "Melee Attack";
    }

    protected override StatusEffect GetStatusEffect(Unit target)
    {
        return null;
    }
}
