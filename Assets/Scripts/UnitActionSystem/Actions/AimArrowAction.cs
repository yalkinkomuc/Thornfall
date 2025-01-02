using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AimArrowAction : BaseAction, ITargetVisualAction
{
    [SerializeField] private float bowRange = 8f;
    [SerializeField] private int actionPointCost = 3;
    [SerializeField] private int damageAmount = 30;
    [SerializeField] private float aimDuration = 1f;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private GameObject arrowPrefab;

    private Unit targetUnit;
    private bool canShootArrow;
    public bool isAttacking = false;
    
    private State state;
    private float stateTimer;

    public event EventHandler OnShootAnimStarted;
    public event EventHandler<OnArrowFiredEventArgs> OnArrowFired;
    public event EventHandler OnShootCompleted;

     public class OnArrowFiredEventArgs : EventArgs
    {
        public Unit shootingUnit;
        public Unit targetUnit;
    }


  private enum State
    {
        Aiming,
        Shooting,
        Cooloff,
    }
  

    protected override void Awake()
    {
        base.Awake();
        
    }


    public override string GetActionName()
    {
        return "Special Arrow";
    }

    
    
private void Update()
    {
        if (!isActive)
        {
            return;
        }

        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Aiming:
                if (targetUnit != null)
                {
                    Vector3 aimDir = (targetUnit.GetUnitWorldPosition() - unit.GetUnitWorldPosition()).normalized;
                    transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotationSpeed);
                    OnShootAnimStarted?.Invoke(this, EventArgs.Empty);
                }
                
                break;

            case State.Shooting:
                if (canShootArrow)
                {
                    ShootArrow();
                    canShootArrow = false;
                    OnShootCompleted?.Invoke(this, EventArgs.Empty);
                }
                break;

            case State.Cooloff:
                break;
        }

        if (stateTimer <= 0f)
        {
            NextState();
        }
    }


    private void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Shooting;
                stateTimer = aimDuration;
                break;
            case State.Shooting:
                state = State.Cooloff;
                stateTimer = 0.35f;
                break;
            case State.Cooloff:
                ActionComplete();
                break;
                
        }
    }
    
    
    private void ShootArrow()
    {
        OnArrowFired?.Invoke(this, new OnArrowFiredEventArgs 
        { 
            shootingUnit = unit,
            targetUnit = targetUnit 
        });

    }

    private void ArrowProjectile_OnHit(object sender, ArrowProjectile.OnArrowHitEventArgs e)
    {
        if (e.targetUnit != null)
        {
            e.targetUnit.Damage(damageAmount);
        }
        
        ((ArrowProjectile)sender).OnArrowHit -= ArrowProjectile_OnHit;
    }
    
    

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        if (isAttacking)
        {
            return;
        }

        targetUnit = GetValidTarget(bowRange);
        
        ActionStart(onActionComplete);
        
        if (targetUnit == null || targetUnit == unit)
        {
            ActionComplete();
            return;
        }

        

        state = State.Aiming;
        stateTimer = aimDuration;
        canShootArrow = true;

        
    }

    
    public override int GetActionPointsCost()
    {
        if (GetValidTarget(bowRange) == null)
        {
            return 0;
        }
        return actionPointCost;
    }

    public int GetDamageAmount()
    {
        return damageAmount;
    }    
    
    private void AnimationEventHandler_OnReloadCompleted(object sender, EventArgs e)
    {
        if (!canShootArrow) return;
        
        if (targetUnit != null)
        {
            // Ok'un yönünü hedef yönüne çevir
            Vector3 aimDir = (targetUnit.transform.position - shootPoint.position).normalized;
            Quaternion arrowRotation = Quaternion.LookRotation(aimDir);
            
            // Ok'u doğru rotasyonla spawn et
            GameObject arrowObject = Instantiate(arrowPrefab, shootPoint.position, arrowRotation);
            ArrowProjectile arrowProjectile = arrowObject.GetComponent<ArrowProjectile>();
            
            Vector3 targetPosition = targetUnit.transform.position;
            targetPosition.y = shootPoint.position.y;
            
            arrowProjectile.Setup(targetPosition, targetUnit);
            arrowProjectile.OnArrowHit += ArrowProjectile_OnHit;
            
            OnArrowFired?.Invoke(this, new OnArrowFiredEventArgs 
            { 
                shootingUnit = unit,
                targetUnit = targetUnit 
            });
        }
        
        canShootArrow = false;
    }

   
  

    public override bool ShouldShowTargetVisual(Unit targetUnit)
    {
        if (!base.ShouldShowTargetVisual(targetUnit)) return false;
        
        List<Unit> validTargets = GetValidTargetListWithSphere(bowRange);
        return validTargets.Contains(targetUnit);
    }
}
