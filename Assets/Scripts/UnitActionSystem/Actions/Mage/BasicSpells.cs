using System;
using System.Collections.Generic;
using UnityEngine;

public class BasicSpells : BaseRangeAction
{
    [Header("Spell Settings")]
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

        hasShot = true;
    }

    private void SpellProjectile_OnHit(object sender, SpellProjectile.OnSpellHitEventArgs e)
    {
        if (e.targetUnit != null)
        {
            e.targetUnit.Damage(e.spellEffect.CalculateDamage());
        }

        SpellProjectile spellProjectile = sender as SpellProjectile;
        if (spellProjectile != null)
        {
            spellProjectile.OnSpellHit -= SpellProjectile_OnHit;
        }
    }

    public override void TakeAction(Vector3 targetPosition, Action onActionComplete)
    {
        targetUnit = GetValidTarget(range);
        if (targetUnit == null)
        {
            onActionComplete?.Invoke();
            return;
        }

        if (!unit.TrySpendActionPointsToTakeAction(this))
        {
            return;
        }

        ActionStart(onActionComplete);
        state = State.Aiming;
        stateTimer = aimDuration;
        isAttacking = true;
        hasShot = false;

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
            if (hit.collider.TryGetComponent<Unit>(out Unit targetUnit))
            {
                if (targetUnit.IsEnemy() && Vector3.Distance(unit.transform.position, targetUnit.transform.position) <= range)
                {
                    return targetUnit;
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
        SpellProjectile[] activeSpells = FindObjectsOfType<SpellProjectile>();
        foreach (SpellProjectile spell in activeSpells)
        {
            if (spell != null)
            {
                spell.OnSpellHit -= SpellProjectile_OnHit;
            }
        }
    }

    private void AnimationEventHandler_OnAttackCompleted(object sender, EventArgs e)
    {
        if (!hasShot) return;
        ActionComplete();
    }
}
