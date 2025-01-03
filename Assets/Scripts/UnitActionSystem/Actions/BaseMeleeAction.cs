using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMeleeAction : BaseAction
{
    [SerializeField] protected virtual float attackRange { get; set; } = 2f;
    [SerializeField] protected virtual float stoppingDistance { get; set; } = 1f;
    [SerializeField] protected virtual float hitForce { get; set; } = 10f;
    
    protected Animator animator;
    protected AnimationEventHandler animationEventHandler;
    protected Unit targetUnit;
    protected MoveAction moveAction;
    protected IPathfinding pathfinding;
    public bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
        animationEventHandler = GetComponentInChildren<AnimationEventHandler>();
        moveAction = GetComponent<MoveAction>();
        pathfinding = GetComponent<PathfindingUtils>();
    }

    protected virtual void Start()
    {
        animationEventHandler.OnAttackCompleted += AnimationEventHandler_OnAttackCompleted;
        animationEventHandler.OnMeleeHit += AnimationEventHandler_OnMeleeHit;
    }

    protected virtual void OnDestroy()
    {
        if (animationEventHandler != null)
        {
            animationEventHandler.OnAttackCompleted -= AnimationEventHandler_OnAttackCompleted;
            animationEventHandler.OnMeleeHit -= AnimationEventHandler_OnMeleeHit;
        }
    }

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        if (isAttacking)
        {
            Debug.Log("Already attacking, returning");
            return;
        }

        targetUnit = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue))
        {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit clickedUnit))
            {
                if (clickedUnit.IsEnemy())
                {
                    targetUnit = clickedUnit;
                    Debug.Log($"Target unit found: {targetUnit.name}");
                }
            }
        }

        if (targetUnit == null)
        {
            Debug.Log("No target unit found, completing action");
            onActionComplete?.Invoke();
            return;
        }

        ActionStart(onActionComplete);

        float distanceToTarget = Vector3.Distance(transform.position, targetUnit.transform.position);
        float effectiveRange = GetAttackRange() + GetStoppingDistance();

        Debug.Log($"Distance to target: {distanceToTarget}, Effective Range: {effectiveRange}");

        if (distanceToTarget <= effectiveRange)
        {
            Debug.Log("Target in range, starting attack");
            isAttacking = true;
            StartAttack();
        }
        else
        {
            float maxMoveRange = moveAction.GetMaxMovementPoints() / moveAction.GetMovementCostPerUnit();
            Debug.Log($"Max move range: {maxMoveRange}");
            
            Vector3 directionToTarget = (targetUnit.transform.position - transform.position).normalized;
            Vector3 targetPos;

            if (distanceToTarget > maxMoveRange)
            {
                targetPos = transform.position + directionToTarget * maxMoveRange;
                Debug.Log($"Moving to max range position: {targetPos}");
            }
            else
            {
                targetPos = targetUnit.transform.position - directionToTarget * GetStoppingDistance();
                Debug.Log($"Moving to stopping distance position: {targetPos}");
            }
            
            moveAction.TakeAction(targetPos, () => {
                float finalDistance = Vector3.Distance(transform.position, targetUnit.transform.position);
                Debug.Log($"Final distance after move: {finalDistance}");
                if (finalDistance <= effectiveRange)
                {
                    Debug.Log("In range after move, starting attack");
                    isAttacking = true;
                    StartAttack();
                }
                else
                {
                    Debug.Log("Still out of range after move, completing action");
                    ActionComplete();
                }
            });
        }
    }

    protected virtual void StartAttack()
    {
        if (!unit.TrySpendActionPointsToTakeAction(this))
        {
            ActionComplete();
            return;
        }

        Vector3 targetDirection = (targetUnit.transform.position - transform.position).normalized;
        transform.forward = targetDirection;
        OnStartAttack();
    }

    protected abstract void OnStartAttack();
    protected abstract int GetDamageAmount();
    protected abstract float GetHitForce();
    protected abstract float GetStoppingDistance();
    protected abstract float GetAttackRange();

    private void AnimationEventHandler_OnAttackCompleted(object sender, EventArgs e)
    {
        Debug.Log("Attack completed, resetting state");
        isAttacking = false;
        targetUnit = null;
        if (isActive)
        {
            ActionComplete();
        }
    }

    private void AnimationEventHandler_OnMeleeHit(object sender, EventArgs e)
    {
        if (targetUnit != null)
        {
            targetUnit.Damage(GetDamageAmount());

            if (targetUnit.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                Vector3 direction = (targetUnit.transform.position - transform.position).normalized;
                direction.y = 0.5f;
                rb.AddForce(direction * GetHitForce(), ForceMode.Impulse);
            }
        }
    }

    public override List<Unit> GetValidTargetListWithSphere(float radius)
    {
        float maxMoveRange = moveAction.GetMaxMovementPoints() / moveAction.GetMovementCostPerUnit();
        return pathfinding.GetValidTargetListWithPath(radius, maxMoveRange);
    }
} 