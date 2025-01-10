using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar_UI : MonoBehaviour
{
    [SerializeField] private Image healthBarFill; // Bar isimli Image component'i
    [SerializeField] private Unit unit;
    
    [SerializeField] private float hideDelay = 2f; // Hasar sonrası görünme süresi
    [SerializeField] private float fadeSpeed = 1.5f; // Kaybolma hızı
    
    private Camera mainCamera;
    private HealthSystem healthSystem;
    private CanvasGroup canvasGroup;
    private float hideTimer;
    private bool isHiding;
    private static readonly KeyCode showAllHealthBarsKey = KeyCode.LeftAlt;

    private void Awake()
    {
        mainCamera = Camera.main;
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        healthSystem = unit.GetComponent<HealthSystem>();
        
        // Bar rengini ayarla (düşman kırmızı, dost yeşil)
        healthBarFill.color = unit.IsEnemy() ? Color.red : Color.green;
        
        // Başlangıçta gizli
        canvasGroup.alpha = 0f;
    }

    private void Start()
    {
        // Event'leri dinle
        healthSystem.OnDamageTaken += HealthSystem_OnDamageTaken;
        UpdateHealthBar();
    }

    private void Update()
    {
        // Health bar'ı kameraya döndür
        transform.rotation = mainCamera.transform.rotation;

        // Alt tuşu basılıysa hepsini göster
        if (Input.GetKey(showAllHealthBarsKey))
        {
            ShowHealthBar();
            return;
        }

        // Mouse unit üzerindeyse göster
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == unit.gameObject)
            {
                ShowHealthBar();
                hideTimer = hideDelay;
                isHiding = false;
                return;
            }
        }

        // Alt tuşu bırakıldığında hemen gizlenmeye başla
        if (Input.GetKeyUp(showAllHealthBarsKey))
        {
            isHiding = true;
            hideTimer = 0;
        }

        if (hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                isHiding = true;
            }
        }

        if (isHiding)
        {
            HideHealthBar();
        }
    }

    private void HealthSystem_OnDamageTaken(object sender, HealthSystem.OnDamageTakenEventArgs e)
    {
        UpdateHealthBar();
        ShowHealthBar();
        hideTimer = hideDelay; // Hasar alınca timer'ı resetle
    }

    private void UpdateHealthBar()
    {
        healthBarFill.fillAmount = healthSystem.GetHealthNormalized();
    }

    private void ShowHealthBar()
    {
        canvasGroup.alpha = 1f;
    }

    private void HideHealthBar()
    {
        canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
        if (canvasGroup.alpha <= 0)
        {
            canvasGroup.alpha = 0;
            isHiding = false;
        }
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken -= HealthSystem_OnDamageTaken;
        }
    }
} 