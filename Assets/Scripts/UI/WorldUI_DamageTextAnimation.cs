using System;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class WorldUI_DamageTextAnimation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI damageText;
    private Camera mainCamera;

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
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
            mainCamera.transform.rotation * Vector3.up);
    }

    public void Setup(int damageAmount)
    {
        Setup(damageAmount, Color.white);
    }

    public void Setup(int damageAmount, Color textColor)
    {
        damageText.text = damageAmount.ToString();
        damageText.color = textColor;
        
        // Yukarı doğru hareket ve fade out animasyonu
        Sequence sequence = DOTween.Sequence();
        sequence.Append(transform.DOLocalMoveY(transform.localPosition.y + moveYDistance, animationDuration)
            .SetEase(Ease.OutCubic));
        sequence.Join(damageText.DOFade(0, fadeOutDuration));
    }
}
