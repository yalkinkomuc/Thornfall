using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

        foreach (Unit target in targetUnits)
        {
            if (target != null && target.gameObject != null)
            {
                SpawnAndShootArrow(target);
                yield return new WaitForSeconds(arrowSpawnDelay);
            }
        }
        allArrowsShot = true;
    }

    protected override void ArrowProjectile_OnHit(object sender, ArrowProjectile.OnArrowHitEventArgs e)
    {
        try 
        {
            if (e.targetUnit != null)
            {
                BaseRangeAction targetRangeAction = e.targetUnit.GetComponent<BaseRangeAction>();
                if (targetRangeAction != null)
                {
                    targetRangeAction.CancelAction();
                }

                e.targetUnit.Damage(GetDamageAmount());
            }
        }
        finally 
        {
            ((ArrowProjectile)sender).OnArrowHit -= ArrowProjectile_OnHit;
            arrowsHitCount++;

            if (allArrowsShot && arrowsHitCount >= targetUnits.Count)
            {
                CompleteAction();
            }
        }
    }

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        if (isAttacking) return;

        Unit newTarget = GetValidTarget(range);
        if (newTarget == null || newTarget == unit)
        {
            return;
        }

        if (targetUnits.Contains(newTarget))
        {
            return;
        }

        if (targetUnits.Count >= MaxTargetCount)
        {
            return;
        }

        targetUnits.Add(newTarget);
        
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
        animator.ResetTrigger("Shoot");
        allArrowsShot = false;
        arrowsHitCount = 0;
        hasShot = false;
        hasStartedShooting = false;
        base.CompleteAction();
    }

    public override void CancelAction()
    {
        animator.ResetTrigger("Shoot");
        allArrowsShot = false;
        arrowsHitCount = 0;
        hasShot = false;
        hasStartedShooting = false;
        base.CancelAction();
    }
}
