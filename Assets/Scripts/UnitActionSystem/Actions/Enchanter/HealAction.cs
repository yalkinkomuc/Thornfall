using System;
using UnityEngine;
using System.Collections.Generic;

public class HealAction : BaseAction, ITargetVisualAction
{
    [Header("Heal Settings")]
    [SerializeField] private int actionPointCost = 2;
    [SerializeField] private int healAmount = 25;
    [SerializeField] private float healRange = 4f;
    [SerializeField] private GameObject healEffectPrefab; // Heal efekti için

    private Unit targetUnit;
    private bool isHealing = false;
    private Animator animator;
    private AnimationEventHandler animationEventHandler;

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

    private void AnimationEventHandler_OnBlessEffect(object sender, EventArgs e)
    {
        if (targetUnit != null)
        {
            // Heal efektini göster
            if (healEffectPrefab != null)
            {
                GameObject healEffect = Instantiate(healEffectPrefab, 
                    targetUnit.transform.position + Vector3.up, 
                    Quaternion.identity);
                var fadeOut = healEffect.AddComponent<FadeOutAndDestroy>();
            }

            // Can yenileme
            targetUnit.Heal(healAmount);
        }
    }

    public override string GetActionName() => "Heal";
    public override int GetActionPointsCost() => actionPointCost;

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        Debug.Log("HealAction: TakeAction başladı");

        // Önce hedefi bul
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit clickedUnit))
            {
                if (!clickedUnit.IsEnemy() && clickedUnit != unit)
                {
                    float distance = Vector3.Distance(unit.transform.position, clickedUnit.transform.position);
                    if (distance <= healRange)
                    {
                        targetUnit = clickedUnit;
                    }
                }
            }
        }

        if (targetUnit == null)
        {
            Debug.Log("HealAction: Hedef unit bulunamadı");
            return; // ActionStart çağrılmadığı için ActionComplete'e gerek yok
        }

        // Action'ı başlat ve action point'leri kontrol et
        ActionStart(onActionComplete);

        Debug.Log($"HealAction: {healAmount} can yenileniyor");
        
        // Sadece animasyonu tetikle
        if (animator != null)
        {
            animator.SetTrigger("Bless");
        }

        Debug.Log("HealAction: Action tamamlanıyor");
        ActionComplete();
        targetUnit = null;

        // Heal action tamamlandıktan sonra MoveAction'a geç
        UnitActionSystem.Instance.SetSelectedAction(unit.GetMoveAction());
    }

    public override List<Unit> GetValidTargetListWithSphere(float radius)
    {
        List<Unit> validTargets = new List<Unit>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, healRange, whatIsUnit);

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

    public override bool ShouldShowTargetVisual(Unit targetUnit)
    {
        if (!base.ShouldShowTargetVisual(targetUnit)) return false;
        
        if (!targetUnit.IsEnemy() && targetUnit != unit)
        {
            float distance = Vector3.Distance(unit.transform.position, targetUnit.transform.position);
            return distance <= healRange;
        }
        return false;
    }
}
