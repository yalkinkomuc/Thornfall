public abstract class StatusEffect
{
    protected Unit targetUnit;
    protected int duration;
    protected int damagePerTurn;
    
    public StatusEffect(Unit target, int duration, int damagePerTurn)
    {
        this.targetUnit = target;
        this.duration = duration;
        this.damagePerTurn = damagePerTurn;
    }

    public abstract void OnTurnStart();
    public abstract string GetEffectName();
    
    public bool IsFinished => duration <= 0;
    
    protected void ReduceDuration()
    {
        duration--;
    }
} 