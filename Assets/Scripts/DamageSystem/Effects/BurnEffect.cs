public class BurnEffect : StatusEffect
{
    public BurnEffect(Unit target, int duration, int damagePerTurn) : base(target, duration, damagePerTurn) { }

    public override void OnTurnStart()
    {
        targetUnit.Damage(damagePerTurn);
        // Yanma efekti particle
        ReduceDuration();
    }

    public override string GetEffectName() => "Burn";
} 