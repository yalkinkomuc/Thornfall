using UnityEngine;

public abstract class StatusEffect
{
    protected Unit targetUnit;
    protected int duration;
    public int damagePerTurn;
    protected Color damageColor;
    
    public StatusEffect(Unit target, int duration, int damagePerTurn, Color damageColor)
    {
        this.targetUnit = target;
        this.duration = duration;
        this.damagePerTurn = damagePerTurn;
        this.damageColor = damageColor;
    }

    public void ProcessTurnStart()
    {
        OnTurnStartEffect();
        ReduceDuration();
    }

    protected virtual void OnTurnStartEffect() { }

    public abstract string GetEffectName();
    public Color GetDamageColor() => damageColor;
    
    public bool IsFinished => duration <= 0;
    
    protected void ReduceDuration()
    {
        duration--;
    }
} 