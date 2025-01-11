using UnityEngine;

public class FadeOutAndDestroy : MonoBehaviour
{
    [SerializeField] private float fadeOutDuration = 1.5f;
    [SerializeField] private float delayBeforeFade = 0.5f;
    
    private ParticleSystem[] particleSystems;
    private float currentTime = 0f;
    private bool isFading = false;
    private bool isWaiting = true;

    private void Start()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (isWaiting)
        {
            currentTime += Time.deltaTime;
            if (currentTime >= delayBeforeFade)
            {
                isWaiting = false;
                currentTime = 0f;
                StartFade();
            }
            return;
        }

        if (isFading)
        {
            currentTime += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(currentTime / fadeOutDuration);
            
            foreach (var ps in particleSystems)
            {
                if (ps != null)
                {
                    var main = ps.main;
                    var startColor = main.startColor.color;
                    startColor.a = 1 - normalizedTime;
                    main.startColor = startColor;

                    var emission = ps.emission;
                    emission.rateOverTime = 0;
                }
            }

            if (normalizedTime >= 1)
            {
                Destroy(gameObject);
            }
        }
    }

    private void StartFade()
    {
        isFading = true;
        foreach (var ps in particleSystems)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.loop = false;
            }
        }
    }
} 