using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    [SerializeField] private GameObject damageTextPrefab; // Damage text prefab'ı
    [SerializeField] private Transform damageTextParent;  // Canvas altında bir parent

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError("Found more than one UI Manager!");
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void ShowDamageText(Vector3 worldPosition, int damageAmount)
    {
        GameObject textObj = Instantiate(damageTextPrefab, damageTextParent);
        textObj.transform.position = worldPosition + new Vector3(0, 2f, 0); // Unit'in üstünde başlat
        
        WorldUI_DamageTextAnimation damageText = textObj.GetComponent<WorldUI_DamageTextAnimation>();
        if (damageText != null)
        {
            damageText.Setup(damageAmount);
            Destroy(textObj, 2f);  // 2 saniye sonra yok et
        }
    }
} 