using UnityEngine;
using TMPro;

public class ActionPointsText : MonoBehaviour
{
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float fadeSpeed = 1f;
    
  [SerializeField] private TextMeshPro textMesh;
    private float timer;
    private bool isDisplaying;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        gameObject.SetActive(false);
    }

    public void ShowText()
    {
        gameObject.SetActive(true);
        textMesh.text = "Action Points Filled!";
        textMesh.color = new Color(0, 1, 0, 1); // Yeşil
        textMesh.alpha = 1f;
        timer = displayDuration;
        isDisplaying = true;
    }

    private void Update()
    {
        if (!isDisplaying) return;

        // Yukarı doğru hareket
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime);

        // Zamanla kaybol
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            textMesh.alpha -= fadeSpeed * Time.deltaTime;
            if (textMesh.alpha <= 0)
            {
                isDisplaying = false;
                gameObject.SetActive(false);
            }
        }

        // Kameraya dön
        transform.rotation = Camera.main.transform.rotation;
    }
} 