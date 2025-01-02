using System;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class WorldUI_DamageTextAnimation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private float moveYDistance = 1f;
    [SerializeField] private float fadeOutDuration = 0.7f;

    
    
    
    
    
    private void Awake()
    {
        if (damageText == null)
        {
            damageText = GetComponent<TextMeshProUGUI>();
        }
        
        
        
    }

    
    public void Setup(int damageAmount)
    {
        damageText.text = damageAmount.ToString();
        damageText.color = new Color(1, 0, 0, 1);
        
        // Yukarı doğru hareket ve fade out animasyonu
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveY(transform.localPosition.y + moveYDistance, animationDuration)
            .SetEase(Ease.OutCubic));
        sequence.Join(damageText.DOFade(0, fadeOutDuration));
    }
}
