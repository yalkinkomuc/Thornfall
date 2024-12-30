using UnityEngine;
using System;

public class AnimationEventHandler : MonoBehaviour
{
    public event EventHandler OnAttackCompleted;
    public event EventHandler OnReloadCompleted;
    public event EventHandler OnMeleeHit;
    // Diğer animation event'leri için yeni event'ler eklenebilir...

    // Animation Event'ler tarafından çağrılacak methodlar
    public void AttackComplete()
    {
        OnAttackCompleted?.Invoke(this, EventArgs.Empty);
    }

    public void ReloadComplete()
    {
        OnReloadCompleted?.Invoke(this, EventArgs.Empty);
    }

    private void MeleeHit()
    {
        OnMeleeHit?.Invoke(this, EventArgs.Empty);
    }
} 