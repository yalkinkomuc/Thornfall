public class BleedEffect : StatusEffect
{
    public BleedEffect(Unit target, int duration, int damagePerTurn) : base(target, duration, damagePerTurn) { }

    public override void OnTurnStart()
    {
        targetUnit.Damage(damagePerTurn);
        // Kanama efekti particle
        ReduceDuration();
    }

    public override string GetEffectName() => "Bleed";
} 