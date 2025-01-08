using UnityEngine;

public class SpellEffect
{
    public DamageType Type { get; private set; }
    public int BaseDamage { get; private set; }
    public int DurationInTurns { get; private set; }
    public Color EffectColor { get; private set; }

    private readonly string effectDescription;

    public SpellEffect(DamageType type, int baseDamage, int duration, Color effectColor, string description)
    {
        Type = type;
        BaseDamage = baseDamage;
        DurationInTurns = duration;
        EffectColor = effectColor;
        effectDescription = description;
    }

    public virtual int CalculateDamage()
    {
        switch (Type)
        {
            case DamageType.Fire:
                return Mathf.RoundToInt(BaseDamage * 1.2f); // Yanma hasarı
            case DamageType.Ice:
                return BaseDamage; // Yavaşlatma + normal hasar
            case DamageType.Poison:
                return Mathf.RoundToInt(BaseDamage * 0.8f); // Zaman içinde daha fazla hasar
            default:
                return BaseDamage;
        }
    }

    public string GetDescription() => effectDescription;
} 