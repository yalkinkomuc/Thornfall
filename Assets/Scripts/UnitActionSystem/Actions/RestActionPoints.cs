using System;
using UnityEngine;
using System.Collections.Generic;

public class RestActionPoints : BaseAction, ITargetVisualAction
{
    [SerializeField] private int actionPointCost = 2;
    [SerializeField] private float restRange = 4f;
    private bool isUsedThisTurn = false;
    private Unit targetUnit;

    protected override void Start()
    {
        base.Start();
        TurnSystem.instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        isUsedThisTurn = false;
        targetUnit = null;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (TurnSystem.instance != null)
        {
            TurnSystem.instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
        }
    }

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        ActionStart(onActionComplete);

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit clickedUnit))
            {
                if (!clickedUnit.IsEnemy() && clickedUnit != unit)
                {
                    float distance = Vector3.Distance(unit.transform.position, clickedUnit.transform.position);
                    if (distance <= restRange)
                    {
                        targetUnit = clickedUnit;
                    }
                }
            }
        }

        if (targetUnit == null || isUsedThisTurn)
        {
            ActionComplete();
            return;
        }

        targetUnit.ResetActionPoints();
        isUsedThisTurn = true;
        
        UnitActionSystem.Instance.RefreshSelectedAction();
        ActionComplete();
        UnitActionSystem.Instance.SetSelectedAction(unit.GetMoveAction());
    }

    public override List<Unit> GetValidTargetListWithSphere(float radius)
    {
        List<Unit> validTargets = new List<Unit>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, restRange, whatIsUnit);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<Unit>(out Unit potentialTarget))
            {
                // Kendisi değil ve düşman olmayan unit'leri listeye ekle
                if (!potentialTarget.IsEnemy() && potentialTarget != unit)
                {
                    validTargets.Add(potentialTarget);
                }
            }
        }

        return validTargets;
    }

    public override Unit GetValidTarget(float radius)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue))
        {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit clickedUnit))
            {
                if (!clickedUnit.IsEnemy() && clickedUnit != unit)
                {
                    float distance = Vector3.Distance(unit.transform.position, clickedUnit.transform.position);
                    if (distance <= restRange)
                    {
                        return clickedUnit;
                    }
                }
            }
        }
        return null;
    }

    public override int GetActionPointsCost()
    {
        return isUsedThisTurn ? 999 : actionPointCost;
    }

    public override string GetActionName()
    {
        return "Rest Ally";
    }

    public bool IsUsedThisTurn()
    {
        return isUsedThisTurn;
    }

    public override bool ShouldShowTargetVisual(Unit targetUnit)
    {
        // Önce base'in kontrolünü yap (mouse üzerinde mi?)
        if (!base.ShouldShowTargetVisual(targetUnit)) return false;
        
        if (isUsedThisTurn) return false;
        
        if (!targetUnit.IsEnemy() && targetUnit != unit)
        {
            float distance = Vector3.Distance(unit.transform.position, targetUnit.transform.position);
            return distance <= restRange;
        }
        return false;
    }
} 