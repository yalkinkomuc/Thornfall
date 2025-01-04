using UnityEngine;

public class HeavyAttack_Berserker : BaseMeleeAction
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private int actionPointCost = 2;
    [SerializeField] private int damageAmount = 60;
    [SerializeField] private float stoppingDistance = 1.5f; // bu ayarlar inspectordan değil scriptten değiştirilebilir
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float hitForce = 10f;
    [SerializeField] private int bleedDamageAmount = 10;
    [SerializeField] private int bleedDurationPerTurn = 1;

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
        return new PoisonEffect(target, bleedDurationPerTurn, bleedDamageAmount); // Berserker heavy attack için yanma efekti
    }
}
