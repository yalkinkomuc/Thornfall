using UnityEngine;

public struct DamageData
{
    public int amount;
    public Color textColor;
    public bool showDamageText;

    public static DamageData Create(int amount, Color? color = null, bool showText = true)
    {
        return new DamageData
        {
            amount = amount,
            textColor = color ?? Color.white,
            showDamageText = showText
        };
    }
} 