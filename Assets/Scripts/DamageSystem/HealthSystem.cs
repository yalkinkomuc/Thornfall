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

    public void Damage(int damageAmount)
    {
        health -= damageAmount;
        
        // Damage text'i UI Manager üzerinden göster
        UIManager.Instance.ShowDamageText(transform.position, damageAmount);
        
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    private void Die()
    {
        WorldUI_DamageTextAnimation damageText = GetComponentInChildren<WorldUI_DamageTextAnimation>();
        if (damageText != null)
        {
            damageText.transform.SetParent(null, worldPositionStays: false);
            Destroy(damageText.gameObject, 2f);
        }

        OnDead?.Invoke(this, EventArgs.Empty);
    }

    public float GetHealthNormalized()
    {
        return (float)health / maxHealth;
    }

    public void Heal(int healAmount)
    {
        health = Mathf.Min(health + healAmount, maxHealth);
    }

    public void DamageWithoutText(int damageAmount)
    {
        health -= damageAmount;
        
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }
}
