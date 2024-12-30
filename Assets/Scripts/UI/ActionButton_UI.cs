using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionButton_UI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private GameObject selectedGameObject;
    [SerializeField] private Image buttonImage;
    
    private BaseAction baseAction;
    
    private void Awake()
    {
        buttonImage = GetComponent<Image>();
    }
    
    public void SetBaseAction(BaseAction baseAction)
    {
        this.baseAction = baseAction;
        textMeshPro.text = baseAction.GetActionName().ToUpper();
        button.onClick.AddListener(() =>
        {
            UnitActionSystem.Instance.SetSelectedAction(baseAction);
        });
        
        if (baseAction is RestActionPoints restAction)
        {
            UpdateRestButtonVisual(restAction);
        }
    }

    public void UpdateSelectedActionVisual()
    {
        BaseAction selectedBaseAction = UnitActionSystem.Instance.GetSelectedAction();
        selectedGameObject.SetActive(selectedBaseAction == baseAction);
        
        if (baseAction is RestActionPoints restAction)
        {
            UpdateRestButtonVisual(restAction);
        }
    }

    private void UpdateRestButtonVisual(RestActionPoints restAction)
    {
        if (restAction.IsUsedThisTurn())
        {
            Color color = buttonImage.color;
            color.a = 0.5f;
            buttonImage.color = color;

            Color textColor = textMeshPro.color;
            textColor.a = 0.5f;
            textMeshPro.color = textColor;
        }
        else
        {
            Color color = buttonImage.color;
            color.a = 1f;
            buttonImage.color = color;

            Color textColor = textMeshPro.color;
            textColor.a = 1f;
            textMeshPro.color = textColor;
        }
    }

    private void Start()
    {
        TurnSystem.instance.OnTurnChanged += TurnSystem_OnTurnChanged;
    }

    private void OnDestroy()
    {
        if (TurnSystem.instance != null)
        {
            TurnSystem.instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
        }
    }

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        // Turn değiştiğinde tüm butonların görünümünü güncelle
        if (baseAction is RestActionPoints restAction)
        {
            UpdateRestButtonVisual(restAction);
        }
    }
}
