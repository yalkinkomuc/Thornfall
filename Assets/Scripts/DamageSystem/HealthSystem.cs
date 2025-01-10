#pragma warning disable CS0114, CS0618, CS0414, CS0067
using UnityEngine;
using System;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;

    public event EventHandler OnDead;
    public event EventHandler<OnDamageTakenEventArgs> OnDamageTaken;

    public class OnDamageTakenEventArgs : EventArgs
    {
        public int damageAmount;
        public Unit targetUnit;
    }

    private void Start()
    {
        health = maxHealth;
    }

    public void Damage(DamageData damageData)
    {
        int previousHealth = health;
        health -= damageData.amount;
        
        OnDamageTaken?.Invoke(this, new OnDamageTakenEventArgs { 
            damageAmount = damageData.amount,
            targetUnit = GetComponent<Unit>()
        });
        
        if (damageData.showDamageText)
        {
            UIManager.Instance.ShowDamageText(transform.position, damageData.amount, damageData.textColor);
        }
        
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    public void Damage(int damageAmount)
    {
        Damage(DamageData.Create(damageAmount));
    }

    private void Die()
    {
        OnDead?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)health / maxHealth;
    }

    public void Heal(int healAmount)
    {
        int previousHealth = health;
        health = Mathf.Min(health + healAmount, maxHealth);
        
        if (health != previousHealth)
        {
            OnDamageTaken?.Invoke(this, new OnDamageTakenEventArgs {
                damageAmount = -(health - previousHealth),
                targetUnit = GetComponent<Unit>()
            });
        }
    }

    public void DamageWithoutText(int damageAmount)
    {
        Damage(DamageData.Create(damageAmount, showText: false));
    }

    public bool WouldDieFromDamage(int damageAmount)
    {
        return (health - damageAmount) <= 0;
    }
}
