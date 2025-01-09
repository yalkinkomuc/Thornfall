using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

public abstract class BaseRangeAction : BaseAction
{
    [Header("Base Range Settings")]
    [SerializeField] protected float range = 8f;
    [SerializeField] protected float aimDuration = 1f;
    [SerializeField] protected float rotationSpeed = 10f;
    [SerializeField] protected Transform shootPointTransform;
    [SerializeField] protected GameObject arrowProjectilePrefab;

    protected Animator animator;
    protected Unit targetUnit;
    public bool isAttacking { get; protected set; }
    protected bool canShootArrow;
    
    protected State state;
    protected float stateTimer;

    public event EventHandler OnShootAnimStarted;
    public event EventHandler<OnArrowFiredEventArgs> OnArrowFired;
    public event EventHandler OnShootCompleted;
    
   
    
    public class OnArrowFiredEventArgs : EventArgs
    {
        public Unit shootingUnit;
        public Unit targetUnit;
    }

    protected enum State
    {
        Aiming,
        Shooting,
        Cooloff,
    }

    protected AnimationEventHandler animationEventHandler;

    protected internal bool hasShot;
    protected internal bool isArrowHit;
    protected internal List<Unit> targetUnits = new List<Unit>();
    protected internal int currentTargetIndex = 0;
    protected virtual int MaxTargetCount => 1;

    protected override void Awake()
    {
        base.Awake();
        animator = GetComponentInChildren<Animator>();
        animationEventHandler = GetComponentInChildren<AnimationEventHandler>();

        // Component kontrolleri
        if (shootPointTransform == null)
        {
            Debug.LogError("shootPointTransform is not assigned on " + gameObject.name);
        }
        if (arrowProjectilePrefab == null)
        {
            Debug.LogError("arrowProjectilePrefab is not assigned on " + gameObject.name);
        }
    }

    protected override void Start()
    {
        base.Start();
        if (animationEventHandler != null)
        {
            animationEventHandler.OnAttackCompleted += AnimationEventHandler_OnAttackCompleted;
            animationEventHandler.OnAimShot += AnimationEventHandler_OnAimShot;
            animationEventHandler.OnReloadCompleted += AnimationEventHandler_OnReloadCompleted;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (animationEventHandler != null)
        {
            animationEventHandler.OnAttackCompleted -= AnimationEventHandler_OnAttackCompleted;
            animationEventHandler.OnAimShot -= AnimationEventHandler_OnAimShot;
            animationEventHandler.OnReloadCompleted -= AnimationEventHandler_OnReloadCompleted;
        }
    }

    protected virtual void AnimationEventHandler_OnAttackCompleted(object sender, EventArgs e)
    {
        animator.ResetTrigger("Shoot");
        hasShot = false;
        isArrowHit = true;
        stateTimer = 0f;
    }

    protected virtual void AnimationEventHandler_OnReloadCompleted(object sender, EventArgs e)
    {
        if (currentTargetIndex < targetUnits.Count)
        {
            hasShot = false;
            isArrowHit = false;
            canShootArrow = true;
        }
    }

    protected virtual void AnimationEventHandler_OnAimShot(object sender, EventArgs e)
    {
        if (hasShot) return;
        
        if (targetUnits == null || targetUnits.Count == 0 || currentTargetIndex >= targetUnits.Count)
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

        SpawnAndShootArrow(currentTarget);
    }

    protected internal virtual void SpawnAndShootArrow(Unit target)
    {
        if (shootPointTransform == null || arrowProjectilePrefab == null)
        {
            Debug.LogError($"Required components missing on {gameObject.name}");
            return;
        }

        GameObject arrowObject = Instantiate(arrowProjectilePrefab, shootPointTransform.position, Quaternion.identity);
        ArrowProjectile arrowProjectile = arrowObject.GetComponent<ArrowProjectile>();

        if (arrowProjectile == null)
        {
            Debug.LogError("ArrowProjectile component not found on prefab");
            Destroy(arrowObject);
            return;
        }

        Vector3 targetPosition = target.GetUnitWorldPosition();
        targetPosition.y = shootPointTransform.position.y;

        arrowProjectile.Setup(targetPosition, target);
        arrowProjectile.OnArrowHit += ArrowProjectile_OnHit;

        OnArrowFired?.Invoke(this, new OnArrowFiredEventArgs 
        { 
            shootingUnit = unit,
            targetUnit = target 
        });
        
        hasShot = true;
    }

    private void Update()
    {
        if (!isActive) return;

        stateTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Aiming:
                HandleAiming();
                break;
            case State.Shooting:
                HandleShooting();
                break;
            case State.Cooloff:
                break;
        }

        if (stateTimer <= 0f)
        {
            NextState();
        }
    }

    protected virtual void HandleAiming()
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

        if (!hasShot)
        {
            Vector3 aimDir = (currentTarget.GetUnitWorldPosition() - unit.GetUnitWorldPosition()).normalized;
            transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * rotationSpeed);
            OnStartAttack();
            OnShootAnimStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    protected virtual void HandleShooting()
    {
        if (currentTargetIndex >= targetUnits.Count)
        {
            CancelAction();
            return;
        }

        Unit currentTarget = targetUnits[currentTargetIndex];
        if (currentTarget == null)
        {
            CancelAction();
            return;
        }

        if (canShootArrow)
        {
            canShootArrow = false;
        }
    }

    protected virtual void NextState()
    {
        switch (state)
        {
            case State.Aiming:
                state = State.Shooting;
                stateTimer = aimDuration;
                break;
            case State.Shooting:
                if (!hasShot)
                {
                    stateTimer = 0.1f;
                    return;
                }
                state = State.Cooloff;
                stateTimer = 0.1f;
                break;
            case State.Cooloff:
                if (!isArrowHit)
                {
                    if (stateTimer < -2f)
                    {
                        Debug.LogWarning("Arrow hit timeout - forcing action complete");
                        CancelAction();
                        return;
                    }
                    stateTimer = 0.1f;
                    return;
                }

                currentTargetIndex++;
                if (currentTargetIndex < targetUnits.Count)
                {
                    // Sonraki hedefe geç
                    state = State.Aiming;
                    hasShot = false;
                    isArrowHit = false;
                    stateTimer = aimDuration;
                }
                else
                {
                    // Tüm hedefler tamamlandı
                    CompleteAction();
                }
                break;
        }
    }

    protected virtual void ArrowProjectile_OnHit(object sender, ArrowProjectile.OnArrowHitEventArgs e)
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
            isArrowHit = true;
            stateTimer = 0f;  // State machine'i hemen tetikle

            // Eğer son hedefse ve ok vurduysa action'ı tamamla
            if (currentTargetIndex >= targetUnits.Count - 1)
            {
                CompleteAction();
            }
        }
    }

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        // Eğer zaten atış yapıyorsa yeni hedef seçimine izin verme
        if (isAttacking) return;

        Unit newTarget = GetValidTarget(range);
        if (newTarget == null || newTarget == unit)
        {
            return;
        }

        // Eğer aynı hedef zaten seçilmişse veya maksimum hedef sayısına ulaşılmışsa
        if (targetUnits.Contains(newTarget) || targetUnits.Count >= MaxTargetCount)
        {
            return;
        }

        targetUnits.Add(newTarget);
        
        // Eğer yeterli hedef seçildiyse veya tek hedef alabiliyorsa aksiyonu başlat
        if (targetUnits.Count >= MaxTargetCount || MaxTargetCount == 1)
        {
            StartAttackSequence(onActionComplete);
        }
    }

    protected virtual void StartAttackSequence(Action onActionComplete)
    {
        // Action'ı başlatmadan önce action point kontrolü yap
        if (!unit.TrySpendActionPointsToTakeAction(this))
        {
            targetUnits.Clear();
            return;
        }

        ActionStart(onActionComplete);
        hasShot = false;
        isArrowHit = false;
        isAttacking = true;
        currentTargetIndex = 0;
        state = State.Aiming;
        stateTimer = aimDuration;
        canShootArrow = true;

        // Busy state'i set et
        UnitActionSystem.Instance.SetBusy();
        // Action başladı event'ini tetikle
        UnitActionSystem.Instance.InvokeActionStarted();
    }

    public override List<Unit> GetValidTargetListWithSphere(float radius)
    {
        return base.GetValidTargetListWithSphere(range);
    }

    public override bool ShouldShowTargetVisual(Unit targetUnit)
    {
        if (!base.ShouldShowTargetVisual(targetUnit)) return false;
        List<Unit> validTargets = GetValidTargetListWithSphere(range);
        return validTargets.Contains(targetUnit);
    }

    public abstract int GetDamageAmount();
    protected virtual float GetRange() => range;
    protected abstract void OnStartAttack();

    public virtual void CompleteAction()
    {
        animator.ResetTrigger("Shoot");
        animator.ResetTrigger("ShootHorizontal");
        hasShot = false;
        isArrowHit = false;
        isAttacking = false;
        currentTargetIndex = 0;
        targetUnits.Clear();
        OnShootCompleted?.Invoke(this, EventArgs.Empty);
        ActionComplete();
    }

    public virtual void CancelAction()
    {
        animator.ResetTrigger("Shoot");
        animator.ResetTrigger("ShootHorizontal");
        isAttacking = false;
        hasShot = false;
        isArrowHit = true;
        currentTargetIndex = 0;
        targetUnits.Clear();
        OnShootCompleted?.Invoke(this, EventArgs.Empty);
        ActionComplete();
    }

    protected void InvokeOnShootAnimStarted()
    {
        OnShootAnimStarted?.Invoke(this, EventArgs.Empty);
    }

    protected void InvokeOnShootCompleted()
    {
        OnShootCompleted?.Invoke(this, EventArgs.Empty);
    }
} 