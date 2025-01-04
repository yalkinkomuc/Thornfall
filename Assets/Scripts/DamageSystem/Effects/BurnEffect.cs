using UnityEngine;

public class BurnEffect : StatusEffect
{
    private static readonly Color burnColor = new Color(1f, 0.5f, 0f); // Turuncu

    public BurnEffect(Unit target, int duration, int damagePerTurn) 
        : base(target, duration, damagePerTurn, burnColor) { }

    protected override void OnTurnStartEffect()
    {
        // Sadece particle effect
    }

    public override string GetEffectName() => "Burn";
} 