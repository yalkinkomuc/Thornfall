using UnityEngine;
using System;

public class SpellProjectile : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 20f;
    
    private Vector3 targetPosition;
    private Unit targetUnit;
    private SpellEffect spellEffect;
    private bool hasHit;

    public event EventHandler<OnSpellHitEventArgs> OnSpellHit;

    public class OnSpellHitEventArgs : EventArgs
    {
        public Unit targetUnit;
        public SpellEffect spellEffect;
    }

    public void Setup(Vector3 targetPosition, Unit targetUnit, SpellEffect effect)
    {
        this.targetUnit = targetUnit;
        this.spellEffect = effect;
        
        // Hedefin kafasını hedefle (basit yükseklik hesabı)
        this.targetPosition = targetUnit.transform.position + Vector3.up * 1.7f;

        // Spell efektinin rengini ayarla
        var particleSystem = GetComponentInChildren<ParticleSystem>();
        if (particleSystem != null)
        {
            var main = particleSystem.main;
            main.startColor = new ParticleSystem.MinMaxGradient(effect.EffectColor);
        }
    }

    private void Update()
    {
        if (hasHit) return;

        transform.position = Vector3.MoveTowards(
            transform.position, 
            targetPosition, 
            fallSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            Hit();
        }
    }

    private void Hit()
    {
        if (hasHit) return;
        hasHit = true;

        OnSpellHit?.Invoke(this, new OnSpellHitEventArgs 
        { 
            targetUnit = targetUnit,
            spellEffect = spellEffect
        });

        // Projectile çarptığında busy'i temizle
        UnitActionSystem.Instance.ClearBusy();

        Destroy(gameObject);
    }
} 