using System;
using UnityEngine;
using System.Collections.Generic;

public class RestActionPoints : BaseAction, ITargetVisualAction
{
    [SerializeField] private int actionPointCost = 2;
    [SerializeField] private float restRange = 4f;
    [SerializeField] private GameObject restEffectPrefab;
    private bool isUsedThisTurn = false;
    private Unit targetUnit;
    private Animator animator;
    private AnimationEventHandler animationEventHandler;
    private bool isRotating = false;
    private float rotationSpeed = 10f;

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
        
        // Turn system event'ini ekleyelim
        TurnSystem.instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (animationEventHandler != null)
        {
            animationEventHandler.OnBlessEffectTriggered -= AnimationEventHandler_OnBlessEffect;
        }
        
        // Turn system event'ini temizleyelim
        if (TurnSystem.instance != null)
        {
            TurnSystem.instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
        }
    }

    private void AnimationEventHandler_OnBlessEffect(object sender, EventArgs e)
    {
        if (targetUnit != null)
        {
            // Rest efektini göster
            if (restEffectPrefab != null)
            {
                GameObject restEffect = Instantiate(restEffectPrefab, 
                    targetUnit.transform.position + Vector3.up, 
                    Quaternion.identity);
                var fadeOut = restEffect.AddComponent<FadeOutAndDestroy>();
            }

            // Action pointleri yenile
            targetUnit.ResetActionPoints();
            isUsedThisTurn = true;
            UnitActionSystem.Instance.RefreshSelectedAction();
        }
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        isUsedThisTurn = false;
        targetUnit = null;
    }

    private void Update()
    {
        if (isRotating && targetUnit != null)
        {
            Vector3 targetDirection = (targetUnit.transform.position - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, targetDirection, Time.deltaTime * rotationSpeed);

            // Dönüş tamamlandı mı kontrol et
            if (Vector3.Dot(transform.forward, targetDirection) > 0.99f)
            {
                isRotating = false;
                // Tam olarak hedefe bak
                transform.forward = targetDirection;
                
                // Animasyonu tetikle
                if (animator != null)
                {
                    animator.SetTrigger("Bless");
                }

                ActionComplete();
                UnitActionSystem.Instance.SetSelectedAction(unit.GetMoveAction());
            }
        }
    }

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        // Önce hedefi bul
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit clickedUnit))
            {
                if (!clickedUnit.IsEnemy() && clickedUnit != unit)
                {
                    float distance = Vector3.Distance(unit.transform.position, clickedUnit.transform.position);
                    if (distance <= restRange)
                    {
                        targetUnit = clickedUnit;
                    }
                }
            }
        }

        if (targetUnit == null || isUsedThisTurn)
        {
            return;
        }

        // Action'ı başlat
        ActionStart(onActionComplete);

        // Dönüşü başlat
        isRotating = true;
    }

    public override List<Unit> GetValidTargetListWithSphere(float radius)
    {
        List<Unit> validTargets = new List<Unit>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, restRange, whatIsUnit);

        foreach (Collider collider in colliders)
        {
            if (collider.TryGetComponent<Unit>(out Unit potentialTarget))
            {
                // Kendisi değil ve düşman olmayan unit'leri listeye ekle
                if (!potentialTarget.IsEnemy() && potentialTarget != unit)
                {
                    validTargets.Add(potentialTarget);
                }
            }
        }

        return validTargets;
    }

    public override Unit GetValidTarget(float radius)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue))
        {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit clickedUnit))
            {
                if (!clickedUnit.IsEnemy() && clickedUnit != unit)
                {
                    float distance = Vector3.Distance(unit.transform.position, clickedUnit.transform.position);
                    if (distance <= restRange)
                    {
                        return clickedUnit;
                    }
                }
            }
        }
        return null;
    }

    public override int GetActionPointsCost()
    {
        return isUsedThisTurn ? 999 : actionPointCost;
    }

    public override string GetActionName()
    {
        return "Rest Ally";
    }

    public bool IsUsedThisTurn()
    {
        return isUsedThisTurn;
    }

    public override bool ShouldShowTargetVisual(Unit targetUnit)
    {
        // Önce base'in kontrolünü yap (mouse üzerinde mi?)
        if (!base.ShouldShowTargetVisual(targetUnit)) return false;
        
        if (isUsedThisTurn) return false;
        
        if (!targetUnit.IsEnemy() && targetUnit != unit)
        {
            float distance = Vector3.Distance(unit.transform.position, targetUnit.transform.position);
            return distance <= restRange;
        }
        return false;
    }
} 