using System;
using UnityEngine;

public class BasicSpells : BaseRangeAction
{
    [SerializeField] private int actionPointsCost = 2;
    [SerializeField] private int damageAmount = 10;
    
    public override string GetActionName()
    {
        return "Basic Spells";
    }

    public override int GetActionPointsCost()
    {
        if (GetValidTarget(GetRange()) == null)
        {
            return 0;
        }
        
        return actionPointsCost;
    }

    public override int GetDamageAmount()
    {
        return damageAmount;
    }

    protected override void Start()
    {
        base.Start();
        OnShootAnimStarted += BasicSpellAction_OnShootAnimStarted;
        
        
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        OnShootAnimStarted -= BasicSpellAction_OnShootAnimStarted;
    }

    private void BasicSpellAction_OnShootAnimStarted(object sender, EventArgs e)
    {
        //
    }
    

    protected override void OnStartAttack()
    {
        animator.SetTrigger("Shoot");
    }
}
