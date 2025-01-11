using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class BaseBlessAction : BaseAction
{
    [Header("Bless Base Settings")]
    [SerializeField] protected float blessRange = 4f;
    [SerializeField] protected GameObject blessEffectPrefab;

    protected Animator animator;
    protected AnimationEventHandler animationEventHandler;
    protected Unit targetUnit;
    protected bool isRotating = false;
    protected float rotationSpeed = 10f;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
        animationEventHandler = GetComponentInChildren<AnimationEventHandler>();
    }

    protected override void Start()
    {
        base.Start();
        if (animationEventHandler != null)
        {
            animationEventHandler.OnBlessEffectTriggered += AnimationEventHandler_OnBlessEffect;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (animationEventHandler != null)
        {
            animationEventHandler.OnBlessEffectTriggered -= AnimationEventHandler_OnBlessEffect;
        }
    }

    protected override void Update()
    {
        base.Update();
        
        if (isRotating && targetUnit != null)
        {
            Vector3 targetDirection = (targetUnit.transform.position - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, targetDirection, Time.deltaTime * rotationSpeed);

            if (Vector3.Dot(transform.forward, targetDirection) > 0.99f)
            {
                isRotating = false;
                transform.forward = targetDirection;
                
                if (animator != null)
                {
                    animator.SetTrigger("Bless");
                }
            }
        }
    }

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        ActionStart(onActionComplete);

        // Önce hedefi bul
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit clickedUnit))
            {
                if (!clickedUnit.IsEnemy() && clickedUnit != unit)
                {
                    float distance = Vector3.Distance(unit.transform.position, clickedUnit.transform.position);
                    if (distance <= blessRange)
                    {
                        targetUnit = clickedUnit;
                    }
                }
            }
        }

        // Hedef geçersizse veya action kullanılamıyorsa
        if (targetUnit == null || !CanPerformBlessAction())
        {
            ActionComplete();
            targetUnit = null;
            return;
        }

        isRotating = true;
    }

    protected virtual void AnimationEventHandler_OnBlessEffect(object sender, EventArgs e)
    {
        if (targetUnit != null)
        {
            // Bless efektini göster
            if (blessEffectPrefab != null)
            {
                GameObject blessEffect = Instantiate(blessEffectPrefab, 
                    targetUnit.transform.position + Vector3.up, 
                    Quaternion.identity);
                var fadeOut = blessEffect.AddComponent<FadeOutAndDestroy>();
            }

            PerformBlessEffect();
            OnBlessComplete();
        }
    }

    protected virtual void OnBlessComplete()
    {
        ActionComplete();
        UnitActionSystem.Instance.SetSelectedAction(unit.GetMoveAction());
        targetUnit = null;
    }

    // Child sınıflar bu metodları override edecek
    protected abstract void PerformBlessEffect();
    protected abstract bool CanPerformBlessAction();

    public override List<Unit> GetValidTargetListWithSphere(float radius)
    {
        List<Unit> validTargets = new List<Unit>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, blessRange, whatIsUnit);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<Unit>(out Unit potentialTarget))
            {
                if (!potentialTarget.IsEnemy() && potentialTarget != unit)
                {
                    validTargets.Add(potentialTarget);
                }
            }
        }

        return validTargets;
    }

    public override bool ShouldShowTargetVisual(Unit targetUnit)
    {
        if (!base.ShouldShowTargetVisual(targetUnit)) return false;
        
        if (!targetUnit.IsEnemy() && targetUnit != unit)
        {
            float distance = Vector3.Distance(unit.transform.position, targetUnit.transform.position);
            return distance <= blessRange;
        }
        return false;
    }
} 