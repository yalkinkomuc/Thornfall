using UnityEngine;

public class PoisonEffect : StatusEffect
{
    private static readonly Color poisonColor = new Color(0f, 0.8f, 0f);

    public PoisonEffect(Unit target, int duration, int damagePerTurn) 
        : base(target, duration, damagePerTurn, poisonColor) { }

    protected override void OnTurnStartEffect()
    {
        // Sadece particle effect
    }

    public override string GetEffectName() => "Poison";
} 