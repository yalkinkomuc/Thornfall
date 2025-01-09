#pragma warning disable CS0114, CS0618, CS0414, CS0067
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class AimArrowAction : BaseRangeAction
{
    [Header("Aim Arrow Settings")]
    [SerializeField] private int actionPointsCost = 3;
    [SerializeField] private int damageAmount = 30;
    [SerializeField] private int maxTargets = 2;
    [SerializeField] private float arrowSpawnDelay = 0.1f;

    private bool allArrowsShot = false;
    private int arrowsHitCount = 0;
    private bool hasStartedShooting = false;

    protected override int MaxTargetCount => maxTargets;

    public override string GetActionName() => "Special Arrow";

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

    protected override void OnStartAttack()
    {
        if (hasStartedShooting) return;
        animator.SetTrigger("Shoot");
    }

    protected override void HandleShooting()
    {
        if (canShootArrow && !hasShot && !hasStartedShooting)
        {
            hasStartedShooting = true;
            StartCoroutine(ShootArrowsToAllTargets());
            canShootArrow = false;
            hasShot = true;
        }
    }

    private IEnumerator ShootArrowsToAllTargets()
    {
        yield return new WaitForSeconds(0.2f);

        float delayBetweenShots = 0.2f;
        bool anyTargetDied = false;

        var targets = new List<Unit>(targetUnits);
        foreach (Unit target in targets)
        {
            if (anyTargetDied || (targetUnits.Count > 0 && !targetUnits[0].gameObject.activeInHierarchy))
            {
                break;
            }

            if (target != null && target.gameObject != null && target.gameObject.activeInHierarchy)
            {
                SpawnAndShootArrow(target);
                yield return new WaitForSeconds(delayBetweenShots);
            }
        }
        allArrowsShot = true;

        yield return new WaitForSeconds(0.5f);
        if (!anyTargetDied)
        {
            CompleteAction();
        }
    }

    protected internal override void SpawnAndShootArrow(Unit target)
    {
        if (shootPointTransform == null || arrowProjectilePrefab == null || target == null)
        {
            return;
        }

        base.SpawnAndShootArrow(target);
    }

    protected override void ArrowProjectile_OnHit(object sender, ArrowProjectile.OnArrowHitEventArgs e)
    {
        try 
        {
            if (e.targetUnit != null && e.targetUnit.gameObject.activeInHierarchy)
            {
                BaseRangeAction targetRangeAction = e.targetUnit.GetComponent<BaseRangeAction>();
                if (targetRangeAction != null)
                {
                    targetRangeAction.CancelAction();
                }

                e.targetUnit.Damage(GetDamageAmount());

                if (!e.targetUnit.gameObject.activeInHierarchy)
                {
                    CompleteAction();
                }
            }
        }
        finally 
        {
            if (sender is ArrowProjectile projectile)
            {
                projectile.OnArrowHit -= ArrowProjectile_OnHit;
            }
            
            arrowsHitCount++;
        }
    }

    public event EventHandler<OnTargetListChangedEventArgs> OnTargetListChanged;

    public class OnTargetListChangedEventArgs : EventArgs
    {
        public int currentTargetCount;
    }

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (targetUnits.Count > 0)
            {
                targetUnits.RemoveAt(targetUnits.Count - 1);
                OnTargetListChanged?.Invoke(this, new OnTargetListChangedEventArgs 
                { 
                    currentTargetCount = targetUnits.Count 
                });
            }
            return;
        }

        if (isAttacking) return;

        Unit newTarget = GetValidTarget(range);
        if (newTarget == null || newTarget == unit)
        {
            return;
        }

        if (targetUnits.Count >= MaxTargetCount)
        {
            return;
        }

        targetUnits.Add(newTarget);
        OnTargetListChanged?.Invoke(this, new OnTargetListChangedEventArgs 
        { 
            currentTargetCount = targetUnits.Count 
        });
        
        if (targetUnits.Count >= MaxTargetCount)
        {
            allArrowsShot = false;
            arrowsHitCount = 0;
            hasShot = false;
            StartAttackSequence(onActionComplete);
        }
    }

    protected override void HandleAiming()
    {
        if (currentTargetIndex >= targetUnits.Count)
        {
            CancelAction();
            return;
        }

        Unit currentTarget = targetUnits[currentTargetIndex];
        if (currentTarget == null || currentTarget.gameObject == null)
        {
            CancelAction();
            return;
        }

        if (!hasShot && !hasStartedShooting)
        {
            Vector3 aimDir = (currentTarget.GetUnitWorldPosition() - unit.GetUnitWorldPosition()).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotationSpeed);
            OnStartAttack();
            InvokeOnShootAnimStarted();
        }
    }

    protected override void AnimationEventHandler_OnAimShot(object sender, EventArgs e)
    {
        Debug.Log("AimArrowAction: Ignoring animation event shot");
    }

    public override void CompleteAction()
    {
        StopAllCoroutines();
        
        animator.ResetTrigger("Shoot");
        allArrowsShot = false;
        arrowsHitCount = 0;
        hasShot = false;
        hasStartedShooting = false;
        isAttacking = false;
        currentTargetIndex = 0;
        
        targetUnits.Clear();
        OnTargetListChanged?.Invoke(this, new OnTargetListChangedEventArgs { currentTargetCount = 0 });
        InvokeOnShootCompleted();
        
        ActionComplete();
    }

    public override void CancelAction()
    {
        animator.ResetTrigger("Shoot");
        allArrowsShot = false;
        arrowsHitCount = 0;
        hasShot = false;
        hasStartedShooting = false;
        isAttacking = false;
        currentTargetIndex = 0;
        targetUnits.Clear();
        
        OnTargetListChanged?.Invoke(this, new OnTargetListChangedEventArgs { currentTargetCount = 0 });
        InvokeOnShootCompleted();
        
        ActionComplete();
    }

    public override bool ShouldShowTargetVisual(Unit targetUnit)
    {
        if (targetUnits.Contains(targetUnit))
        {
            return true;
        }

        if (!base.ShouldShowTargetVisual(targetUnit)) return false;

        if (targetUnits.Count < MaxTargetCount)
        {
            List<Unit> validTargets = GetValidTargetListWithSphere(range);
            return validTargets.Contains(targetUnit);
        }

        return false;
    }
}
