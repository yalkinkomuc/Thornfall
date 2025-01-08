using UnityEngine;

public class SpellStatusEffect : StatusEffect
{
    private SpellEffect spellEffect;
    private int remainingTurns;

    public SpellStatusEffect(Unit targetUnit, SpellEffect effect) : base(targetUnit, effect.BaseDamage, effect.DurationInTurns, effect.EffectColor)
    {
        spellEffect = effect;
        remainingTurns = effect.DurationInTurns;
        damagePerTurn = effect.BaseDamage;
    }

    public override string GetEffectName()
    {
        return spellEffect.Type.ToString() + " Effect";
    }

    protected override void OnTurnStartEffect()
    {
        if (remainingTurns <= 0)
        {
            duration = 0;
            return;
        }

        damagePerTurn = spellEffect.CalculateDamage();
        remainingTurns--;

        // Hasar tipine göre ekstra etkiler
        switch (spellEffect.Type)
        {
            case DamageType.Fire:
                damagePerTurn = Mathf.RoundToInt(damagePerTurn * (1f + (3 - remainingTurns) * 0.2f));
                break;
            case DamageType.Ice:
                damagePerTurn = Mathf.RoundToInt(damagePerTurn * 0.8f);
                break;
            case DamageType.Poison:
                damagePerTurn = Mathf.RoundToInt(damagePerTurn * (1f + remainingTurns * 0.3f));
                break;
        }
    }
} 