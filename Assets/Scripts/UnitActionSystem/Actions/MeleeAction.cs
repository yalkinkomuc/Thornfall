using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class MeleeAction : BaseAction, ITargetVisualAction
{
    [SerializeField] private int actionPointCost = 1;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int damageAmount = 40;
    [SerializeField] private float stoppingDistance = 1f;
    [SerializeField] private float hitForce = 10f;
    
    private Animator animator;
    private AnimationEventHandler animationEventHandler;
    private Unit targetUnit;
    private MoveAction moveAction;
    private bool isAttacking = false;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
        animationEventHandler = GetComponentInChildren<AnimationEventHandler>();
        moveAction = GetComponent<MoveAction>();
    }

    private void Start()
    {
        animationEventHandler.OnAttackCompleted += AnimationEventHandler_OnAttackCompleted;
        animationEventHandler.OnMeleeHit += AnimationEventHandler_OnMeleeHit;
    }

    private void OnDestroy()
    {
        if (animationEventHandler != null)
        {
            animationEventHandler.OnAttackCompleted -= AnimationEventHandler_OnAttackCompleted;
            animationEventHandler.OnMeleeHit -= AnimationEventHandler_OnMeleeHit;
        }
    }

    public override List<Unit> GetValidTargetListWithSphere(float radius)
    {
        List<Unit> validTargets = new List<Unit>();
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        foreach (Unit potentialTarget in allUnits)
        {
            if (!potentialTarget.IsEnemy()) continue;

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(unit.GetUnitWorldPosition(), potentialTarget.GetUnitWorldPosition(), NavMesh.AllAreas, path))
            {
                float pathLength = CalculatePathLength(path.corners);
                float maxMoveRange = moveAction.GetMaxMovementPoints() / moveAction.GetMovementCostPerUnit();

                if (pathLength <= maxMoveRange + attackRange)
                {
                    validTargets.Add(potentialTarget);
                }
            }
        }

        return validTargets;
    }

    private float CalculatePathLength(Vector3[] pathPoints)
    {
        float length = 0;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            length += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
        }
        return length;
    }

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        if (isAttacking)
        {
            return;
        }

        ActionStart(onActionComplete);

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
            ActionComplete();
            return;
        }

        float distanceToTarget = Vector3.Distance(transform.position, targetUnit.transform.position);

        if (distanceToTarget <= attackRange)
        {
            isAttacking = true;
            StartAttack();
        }
        else
        {
            Vector3 targetPos = targetUnit.transform.position - 
                (targetUnit.transform.position - transform.position).normalized * stoppingDistance;
            
            moveAction.TakeAction(targetPos, () => {
                float finalDistance = Vector3.Distance(transform.position, targetUnit.transform.position);
                if (finalDistance <= attackRange)
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

    private void StartAttack()
    {
        Vector3 targetDirection = (targetUnit.transform.position - transform.position).normalized;
        transform.forward = targetDirection;

        animator.SetTrigger("Attack");
    }

    private void AnimationEventHandler_OnAttackCompleted(object sender, EventArgs e)
    {
        isAttacking = false;
        ActionComplete();
    }

    private void AnimationEventHandler_OnMeleeHit(object sender, EventArgs e)
    {
        if (targetUnit != null)
        {
            targetUnit.Damage(damageAmount);

            if (targetUnit.TryGetComponent<Rigidbody>(out Rigidbody rb))
            {
                Vector3 direction = (targetUnit.transform.position - transform.position).normalized;
                direction.y = 0.5f;
                rb.AddForce(direction * hitForce, ForceMode.Impulse);
            }
        }
    }

    public override int GetActionPointsCost()
    {
        return actionPointCost;
    }

    public override string GetActionName()
    {
        return "Melee";
    }

    public bool ShouldShowTargetVisual(Unit targetUnit)
    {
        List<Unit> validTargets = GetValidTargetListWithSphere(attackRange);
        return validTargets.Contains(targetUnit);
    }

    public float GetAttackRange()
    {
        return attackRange;
    }

    public override Unit GetValidTarget(float radius)
    {
        List<Unit> validTargets = GetValidTargetListWithSphere(radius);
        if (validTargets.Count == 0) return null;

        Unit nearestTarget = null;
        float nearestDistance = float.MaxValue;

        foreach (Unit target in validTargets)
        {
            float distance = Vector3.Distance(unit.GetUnitWorldPosition(), target.GetUnitWorldPosition());
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestTarget = target;
            }
        }

        return nearestTarget;
    }
} 