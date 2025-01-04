using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseMeleeAction : BaseAction
{
    [Header("Melee Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] private float hitForce = 20f;
    
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

    protected override void Start()
    {
        base.Start();
        animationEventHandler.OnAttackCompleted += AnimationEventHandler_OnAttackCompleted;
        animationEventHandler.OnMeleeHit += AnimationEventHandler_OnMeleeHit;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
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
                    Debug.Log($"Target unit health: {targetUnit.HealthSystem.GetHealthNormalized()}"); //
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
    protected virtual float GetHitForce() => hitForce;
    protected virtual float GetStoppingDistance() => stoppingDistance;
    protected virtual float GetAttackRange() => attackRange;
    protected abstract StatusEffect GetStatusEffect(Unit target);

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
            int damage = GetDamageAmount();
            
            if (targetUnit.TryGetComponent<UnitRagdollSpawner>(out var ragdollSpawner))
            {
                Vector3 hitDirection = (targetUnit.transform.position - transform.position).normalized;
                hitDirection.y = 0.3f;
                ragdollSpawner.SetLastHitInfo(hitDirection, GetHitForce());
            }

            targetUnit.Damage(damage);

            StatusEffect effect = GetStatusEffect(targetUnit);
            if (effect != null)
            {
                targetUnit.AddStatusEffect(effect);
            }
        }
    }

    public override List<Unit> GetValidTargetListWithSphere(float radius)
    {
        float maxMoveRange = moveAction.GetMaxMovementPoints() / moveAction.GetMovementCostPerUnit();
        return pathfinding.GetValidTargetListWithPath(radius, maxMoveRange);
    }
} 