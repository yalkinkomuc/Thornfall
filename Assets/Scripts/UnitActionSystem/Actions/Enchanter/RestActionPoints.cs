using System;
using UnityEngine;
using System.Collections.Generic;

public class RestActionPoints : BaseBlessAction
{
    private bool isUsedThisTurn = false;

    protected override void Start()
    {
        base.Start();
        TurnSystem.instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (TurnSystem.instance != null)
        {
            TurnSystem.instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
        }
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        isUsedThisTurn = false;
        targetUnit = null;
    }

    protected override void PerformBlessEffect()
    {
        targetUnit.ResetActionPoints();
        isUsedThisTurn = true;
        UnitActionSystem.Instance.RefreshSelectedAction();
    }

    protected override bool CanPerformBlessAction()
    {
        return !isUsedThisTurn;
    }

    public override string GetActionName() => "Rest Ally";

    public bool IsUsedThisTurn()
    {
        return isUsedThisTurn;
    }
} 