using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AimArrowAction : BaseAction, ITargetVisualAction
{
    [SerializeField] private int actionPointCost = 3;
    [SerializeField] private float bowRange = 8f;
    [SerializeField] private int damageAmount = 30;
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private Transform shootPoint;

    private Unit targetUnit;
    private Animator animator;
    private AnimationEventHandler animationEventHandler;
    private bool canShootArrow;
    public bool isAttacking = false;

    public event EventHandler OnShootAnimStarted;
    public event EventHandler<OnArrowFiredEventArgs> OnArrowFired;
    public event EventHandler OnShootCompleted;

    public class OnArrowFiredEventArgs : EventArgs
    {
        public Unit shootingUnit;
        public Unit targetUnit;
    }

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
        animationEventHandler = GetComponentInChildren<AnimationEventHandler>();
    }

    private void Start()
    {
        if (animationEventHandler != null)
        {
            animationEventHandler.OnReloadCompleted += AnimationEventHandler_OnReloadCompleted;
        }
    }

    private void OnDestroy()
    {
        if (animationEventHandler != null)
        {
            animationEventHandler.OnReloadCompleted -= AnimationEventHandler_OnReloadCompleted;
        }
    }

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        if (isAttacking)
        {
            return;
        }

        targetUnit = GetValidTarget(bowRange);
        
        ActionStart(onActionComplete);

        if (targetUnit == null)
        {
            ActionComplete();
            return;
        }

        Vector3 aimDir = (targetUnit.transform.position - transform.position).normalized;
        transform.forward = aimDir;

        canShootArrow = true;
        animator.SetTrigger("Shoot");
        OnShootAnimStarted?.Invoke(this, EventArgs.Empty);
    }

    private void AnimationEventHandler_OnReloadCompleted(object sender, EventArgs e)
    {
        if (!canShootArrow) return;
        
        if (targetUnit != null)
        {
            GameObject arrowObject = Instantiate(arrowPrefab, shootPoint.position, Quaternion.identity);
            ArrowProjectile arrowProjectile = arrowObject.GetComponent<ArrowProjectile>();
            
            Vector3 shootTargetPosition = targetUnit.transform.position;
            shootTargetPosition.y = shootPoint.position.y;
            
            arrowProjectile.Setup(shootTargetPosition, targetUnit);
            arrowProjectile.OnArrowHit += ArrowProjectile_OnHit;
            
            OnArrowFired?.Invoke(this, new OnArrowFiredEventArgs 
            { 
                shootingUnit = unit,
                targetUnit = targetUnit 
            });
        }
        
        canShootArrow = false;
        OnShootCompleted?.Invoke(this, EventArgs.Empty);
        ActionComplete();
    }

    private void ArrowProjectile_OnHit(object sender, ArrowProjectile.OnArrowHitEventArgs e)
    {
        if (e.targetUnit != null)
        {
            e.targetUnit.Damage(damageAmount);
        }
        
        ((ArrowProjectile)sender).OnArrowHit -= ArrowProjectile_OnHit;
    }

    public override string GetActionName()
    {
        return "Special Arrow";
    }

    public override int GetActionPointsCost()
    {
        if (GetValidTarget(bowRange) == null)
        {
            return 0;
        }
        return actionPointCost;
    }

    public override bool ShouldShowTargetVisual(Unit targetUnit)
    {
        if (!base.ShouldShowTargetVisual(targetUnit)) return false;
        
        List<Unit> validTargets = GetValidTargetListWithSphere(bowRange);
        return validTargets.Contains(targetUnit);
    }
}
