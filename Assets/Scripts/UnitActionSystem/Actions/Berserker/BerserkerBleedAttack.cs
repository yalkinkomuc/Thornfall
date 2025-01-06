using UnityEngine;

public class BerserkerBleedAttack : BaseMeleeAction
{
    [Header("Bleed Attack Settings")]
    [SerializeField] private int damageAmount = 25;
    [SerializeField] private int bleedDuration = 3;
    [SerializeField] private int bleedDamagePerTurn = 10;
    [SerializeField] private int actionPointsCost = 2;

    public override string GetActionName()
    {
        return "Bleed Attack";
    }

    public override int GetActionPointsCost()
    {
        return actionPointsCost;
    }

    protected override void OnStartAttack()
    {
        animator.SetTrigger("BleedAttack");
    }

    protected override int GetDamageAmount()
    {
        return damageAmount;
    }

    protected override StatusEffect GetStatusEffect(Unit target)
    {
        return new BleedEffect(target, bleedDuration, bleedDamagePerTurn);
    }

    public override string GetActionDescription()
    {
        return $"Deal {damageAmount} damage and apply bleeding effect that deals {bleedDamagePerTurn} damage for {bleedDuration} turns.";
    }
}
