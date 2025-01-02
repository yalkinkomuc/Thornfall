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
        if (isAttacking) return;

        targetUnit = null;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue))
        {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit clickedUnit))
            {
                if (clickedUnit.IsEnemy())
                {
                    targetUnit = clickedUnit;
                }
            }
        }

        if (targetUnit == null)
        {
            onActionComplete?.Invoke();
            return;
        }

        ActionStart(onActionComplete);

        float distanceToTarget = Vector3.Distance(transform.position, targetUnit.transform.position);
        float effectiveRange = GetAttackRange() + GetStoppingDistance();

        if (distanceToTarget <= effectiveRange)
        {
            isAttacking = true;
            StartAttack();
        }
        else
        {
            Vector3 targetPos = targetUnit.transform.position - 
                (targetUnit.transform.position - transform.position).normalized * GetStoppingDistance();
            
            moveAction.TakeAction(targetPos, () => {
                float finalDistance = Vector3.Distance(transform.position, targetUnit.transform.position);
                if (finalDistance <= effectiveRange)
                {
                    isAttacking = true;
                    StartAttack();
                }
                else
                {
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