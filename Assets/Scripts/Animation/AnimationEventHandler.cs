using UnityEngine;
using System;

public class AnimationEventHandler : MonoBehaviour
{
    public event EventHandler OnAttackCompleted;
    public event EventHandler OnReloadCompleted;
    public event EventHandler OnMeleeHit;
    public event EventHandler OnAimShot;
    public event EventHandler OnWeaponFlip;
    // Diğer animation event'leri için yeni event'ler eklenebilir...

    // Animation Event'ler tarafından çağrılacak methodlar
    public void AttackComplete()
    {
        Debug.Log("Attack Complete Event Triggered");
        OnAttackCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void ReloadComplete()
    {
        Debug.Log("Reload Complete Event Triggered");
        OnReloadCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void MeleeHit()
    {
        Debug.Log("Melee Hit Event Triggered");
        OnMeleeHit?.Invoke(this, EventArgs.Empty);
    }

    public void AimShot()
    {
        OnAimShot?.Invoke(this, EventArgs.Empty);
    }

    
} 