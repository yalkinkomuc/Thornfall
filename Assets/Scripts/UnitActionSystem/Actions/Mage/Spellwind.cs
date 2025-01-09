#pragma warning disable CS0114, CS0618, CS0414, CS0067
using System;
using System.Collections.Generic;
using UnityEngine;

public class SpellWind : BaseRangeAction
{
    [Header("Horizontal Spell Settings")]
    [SerializeField] private int actionPointsCost = 2;
    [SerializeField] private int baseDamage = 12;
    [SerializeField] private GameObject spellProjectilePrefab;
    [SerializeField] private DamageType spellType = DamageType.Ice;

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

        if (shootPointTransform != null && targetUnit != null)
        {
            Vector3 spawnPosition = shootPointTransform.position;
            GameObject spellObject = Instantiate(spellProjectilePrefab, spawnPosition, transform.rotation);
            SpellProjectile spellProjectile = spellObject.GetComponent<SpellProjectile>();

            if (spellProjectile != null)
            {
                SpellEffect effect = new SpellEffect(
                    spellType,
                    baseDamage,
                    1,
                    damageColors[spellType],
                    effectDescriptions[spellType]
                );

                Vector3 targetPos = targetUnit.transform.position;
                spellProjectile.Setup(targetPos, targetUnit, effect);
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
                ActionComplete();
                return;
            }

            e.targetUnit.Damage(e.spellEffect.CalculateDamage());
        }

        if (projectile != null)
        {
            projectile.OnSpellHit -= SpellProjectile_OnHit;
        }

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

        // Action point kontrolünü kaldırdık
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
        animator.SetTrigger("ShootHorizontal");
    }

    public override string GetActionName() => "Spellwind";
    public override int GetActionPointsCost() => actionPointsCost;
    public override int GetDamageAmount() => baseDamage;
    

    

   

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
}
