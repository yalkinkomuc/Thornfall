using System;
using UnityEngine;

public class UnitAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (TryGetComponent<MoveAction>(out MoveAction moveAction))
        {
            moveAction.OnStartMoving += MoveAction_OnStartMoving;
            moveAction.OnStopMoving += MoveAction_OnStopMoving;
        }

        if (TryGetComponent<BowRangeAction>(out BowRangeAction bowRangeAction))
        {
            bowRangeAction.OnShootAnimStarted += BowRangeAction_OnShootAnimStarted;
            bowRangeAction.OnShootCompleted += BowRangeAction_OnShootCompleted;
        }

        if (TryGetComponent<AimArrowAction>(out AimArrowAction aimArrowAction))
        {
            aimArrowAction.OnShootAnimStarted += AimArrowAction_OnShootAnimStarted;
            aimArrowAction.OnShootCompleted += AimArrowAction_OnShootCompleted;
        }
    }

    private void OnDestroy()
    {
        if (TryGetComponent<BowRangeAction>(out BowRangeAction bowRangeAction))
        {
            bowRangeAction.OnShootAnimStarted -= BowRangeAction_OnShootAnimStarted;
            bowRangeAction.OnShootCompleted -= BowRangeAction_OnShootCompleted;
        }

        if (TryGetComponent<AimArrowAction>(out AimArrowAction aimArrowAction))
        {
            aimArrowAction.OnShootAnimStarted -= AimArrowAction_OnShootAnimStarted;
            aimArrowAction.OnShootCompleted -= AimArrowAction_OnShootCompleted;
        }
    }

  


    #region Bow Range Action
    
    private void BowRangeAction_OnShootAnimStarted(object sender, EventArgs e)
    {
        animator.SetTrigger("Shoot");
    }
    
    private void BowRangeAction_OnShootCompleted(object sender, EventArgs e)
    {
        animator.ResetTrigger("Shoot");
    }

#endregion
   

    #region  Move Anim Region

    private void MoveAction_OnStartMoving(object sender, EventArgs e)
    {
        animator.SetBool("isMoving", true);
    }

    private void MoveAction_OnStopMoving(object sender, EventArgs e)
    {
        animator.SetBool("isMoving", false);
    }

    #endregion

    private void AimArrowAction_OnShootAnimStarted(object sender, EventArgs e)
    {
        animator.SetTrigger("Shoot");
    }

    private void AimArrowAction_OnShootCompleted(object sender, EventArgs e)
    {
        animator.ResetTrigger("Shoot");
    }


}