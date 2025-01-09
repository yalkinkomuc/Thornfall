#pragma warning disable CS0414, CS0618, CS0067, CS0114
using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicSpells : BaseRangeAction
{
    [Header("Vertical Spell Settings")]
    [SerializeField] private int actionPointsCost = 2;
    [SerializeField] private int baseDamage = 15;
    [SerializeField] private float spellHeight = 10f;
    [SerializeField] private GameObject spellProjectilePrefab;
    [SerializeField] private DamageType spellType = DamageType.Fire;

    private static readonly Dictionary<DamageType, Color> damageColors = new Dictionary<DamageType, Color>()
    {
        { DamageType.Physical, Color.grey },
        { DamageType.Fire, new Color(1f, 0.5f, 0f) },
        { DamageType.Ice, new Color(0.5f, 0.8f, 1f) },
        { DamageType.Poison, new Color(0.4f, 0.8f, 0.2f) }
    };

    

    private static readonly Dictionary<DamageType, string> effectDescriptions = new Dictionary<DamageType, string>()
    {
        { DamageType.Physical, "Deals pure physical damage" },
        { DamageType.Fire, "Burns the target, dealing additional damage over time" },
        { DamageType.Ice, "Slows the target and deals frost damage" },
        { DamageType.Poison, "Poisons the target, dealing increasing damage over time" }
    };


 protected override void Start()
    {
        base.Start();
        if (animationEventHandler != null)
        {
            animationEventHandler.OnAimShot += AnimationEventHandler_OnAimShot;
            animationEventHandler.OnAttackCompleted += AnimationEventHandler_OnAttackCompleted;
        }
    }


    protected override void Update()
    {
        base.Update();
        if (!isAttacking) return;
        if (targetUnit == null || !targetUnit.gameObject.activeInHierarchy)
        {
            // Hedef öldüyse veya yok olduysa action'ı bitir
            isAttacking = false;
            return;
        }

        switch (state)
        {
            case State.Aiming:
                Vector3 targetDirection = (targetUnit.transform.position - transform.position).normalized;
                transform.forward = Vector3.Lerp(transform.forward, targetDirection, Time.deltaTime * rotationSpeed);
                break;
        }
    }

    private void AnimationEventHandler_OnAimShot(object sender, EventArgs e)
    {
        if (hasShot) return;

        if (targetUnit != null)
        {
            Vector3 spawnPosition = targetUnit.transform.position + Vector3.up * spellHeight;
            GameObject spellObject = Instantiate(spellProjectilePrefab, spawnPosition, Quaternion.identity);
            SpellProjectile spellProjectile = spellObject.GetComponent<SpellProjectile>();

            if (spellProjectile != null)
            {
                SpellEffect effect = new SpellEffect(
                    spellType,
                    baseDamage,
                    3,
                    damageColors[spellType],
                    effectDescriptions[spellType]
                );

                spellProjectile.Setup(targetUnit.transform.position, targetUnit, effect);
                spellProjectile.OnSpellHit += SpellProjectile_OnHit;
            }
        }

        hasShot = true;
    }

    private void SpellProjectile_OnHit(object sender, SpellProjectile.OnSpellHitEventArgs e)
    {
        var projectile = sender as SpellProjectile;

        if (e.targetUnit != null)
        {
            if (!unit.TrySpendActionPointsToTakeAction(this))
            {
                if (projectile != null)
                {
                    projectile.OnSpellHit -= SpellProjectile_OnHit;
                }
                UnitActionSystem.Instance.ClearBusy();
                ActionComplete();
                return;
            }

            e.targetUnit.Damage(e.spellEffect.CalculateDamage());
        }

        if (projectile != null)
        {
            projectile.OnSpellHit -= SpellProjectile_OnHit;
        }

        UnitActionSystem.Instance.ClearBusy();
        ActionComplete();
    }

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        targetUnit = GetValidTarget(range);
        if (targetUnit == null)
        {
            onActionComplete?.Invoke();
            return;
        }

        ActionStart(onActionComplete);
        state = State.Aiming;
        stateTimer = aimDuration;
        isAttacking = true;
        hasShot = false;

        UnitActionSystem.Instance.SetBusy();
        UnitActionSystem.Instance.InvokeActionStarted();

        OnStartAttack();
    }

    protected override void OnStartAttack()
    {
        animator.SetTrigger("Shoot");
    }

    public override string GetActionName() => "Basic Spells";
    public override int GetActionPointsCost() => actionPointsCost;
    public override int GetDamageAmount() => baseDamage;
    

    private Unit GetValidTarget(float range)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Tıklanan noktanın etrafındaki birimleri kontrol et
            var colliders = Physics.OverlapSphere(hit.point, 1f);
            foreach (var collider in colliders)
            {
                if (collider.TryGetComponent<Unit>(out Unit targetUnit))
                {
                    if (targetUnit.IsEnemy() && Vector3.Distance(unit.transform.position, targetUnit.transform.position) <= range)
                    {
                        return targetUnit;
                    }
                }
            }
        }
        return null;
    }

   

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (animationEventHandler != null)
        {
            animationEventHandler.OnAimShot -= AnimationEventHandler_OnAimShot;
            animationEventHandler.OnAttackCompleted -= AnimationEventHandler_OnAttackCompleted;
        }

        // Aktif büyüleri temizle
        var activeSpells = FindObjectsByType<SpellProjectile>(FindObjectsSortMode.None);
        foreach (var spell in activeSpells)
        {
            if (spell != null)
            {
                spell.OnSpellHit -= SpellProjectile_OnHit;
            }
        }
    }

    public override bool ShouldShowTargetVisual(Unit targetUnit)
    {
        // Sadece mesafe ve düşman kontrolü yap
        return targetUnit != null && 
               targetUnit.IsEnemy() && 
               
               Vector3.Distance(unit.transform.position, targetUnit.transform.position) <= range;
    }
}
#pragma warning restore CS0414, CS0618, CS0067, CS0114
