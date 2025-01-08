using UnityEngine;

public class FireSpell : ElementalSpells
{
    
    private static readonly Color fireColor = Color.yellow;
    
    public FireSpell(Unit targetUnit, int damage) : base(targetUnit, damage, fireColor)
    {
    }

    protected override void OnTurnStartEffect()
    {
        // effect
    }

    public override string GetElementalDamageName() => "Fire";
    
        
    
}
