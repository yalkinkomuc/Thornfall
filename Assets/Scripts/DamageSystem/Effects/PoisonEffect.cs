public class PoisonEffect : StatusEffect
{
    public PoisonEffect(Unit target, int duration, int damagePerTurn) : base(target, duration, damagePerTurn) { }

    public override void OnTurnStart()
    {
        targetUnit.Damage(damagePerTurn);
        // Zehir efekti particle
        ReduceDuration();
    }

    public override string GetEffectName() => "Poison";
} 