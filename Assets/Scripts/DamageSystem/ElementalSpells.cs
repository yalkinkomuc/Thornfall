using UnityEngine;

public abstract class ElementalSpells
{
    protected Unit targetUnit;
    protected int damage;
    protected Color elementalDamageColor;

    public ElementalSpells(Unit targetUnit, int damage, Color elementalDamageColor)
    {
        this.targetUnit = targetUnit;
        this.damage = damage;
        this.elementalDamageColor = elementalDamageColor;
        
    }

    public void ProcessTurnStart()
    {
        OnTurnStartEffect();
    }
    
    protected virtual void OnTurnStartEffect(){ }


    public abstract string GetElementalDamageName();
    
    public Color GetElementalDamageColor() => elementalDamageColor;
    
    
}
