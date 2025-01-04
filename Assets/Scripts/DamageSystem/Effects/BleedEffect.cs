using UnityEngine;

public class BleedEffect : StatusEffect
{
    private static readonly Color bleedColor = Color.red;

    public BleedEffect(Unit target, int duration, int damagePerTurn) 
        : base(target, duration, damagePerTurn, bleedColor) { }

    protected override void OnTurnStartEffect()
    {
        // Sadece particle effect
    }

    public override string GetEffectName() => "Bleed";
} 