using UnityEngine;

public class BasicStabRogue : BaseMeleeAction
{
    [Header("Stab Attack Settings")]
    [SerializeField] private int damageAmount = 20;
    [SerializeField] private int actionPointsCost = 1;

    protected override void Awake()
    {
        base.Awake();
        isDefaultCombatAction = true;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    public override string GetActionName()
    {
        return "Stab";
    }

    public override int GetActionPointsCost()
    {
        return actionPointsCost;
    }

    protected override void OnStartAttack()
    {
        animator.SetTrigger("Stab");
    }

    protected override int GetDamageAmount()
    {
        return damageAmount;
    }

    protected override StatusEffect GetStatusEffect(Unit target)
    {
        return null;
    }

    public override string GetActionDescription()
    {
        return $"Quick stab attack that deals {damageAmount} damage.";
    }
}
