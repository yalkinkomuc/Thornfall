using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ActionButton_UI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private GameObject selectedGameObject;
    [SerializeField] private Image buttonImage;
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;
    
    private BaseAction baseAction;
    
    private void Awake()
    {
        buttonImage = GetComponent<Image>();
        
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
        
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => { ShowTooltip(); });
        trigger.triggers.Add(enterEntry);
        
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => { HideTooltip(); });
        trigger.triggers.Add(exitEntry);
        
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
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

    private void ShowTooltip()
    {
        if (tooltipPanel != null && tooltipText != null && baseAction != null)
        {
            tooltipText.text = baseAction.GetActionDescription();
            tooltipText.textWrappingMode = TextWrappingModes.Normal;
            tooltipText.overflowMode = TextOverflowModes.Overflow;
            tooltipText.enableAutoSizing = true;
            tooltipText.fontSizeMin = 12;
            tooltipText.fontSizeMax = 14;
            tooltipText.alignment = TextAlignmentOptions.Center;
            
            // Panel'i button'ın üstüne konumlandır
            RectTransform panelRect = tooltipPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                panelRect.pivot = new Vector2(0.5f, 0);
                panelRect.anchoredPosition = new Vector2(0, 10);
            }
            
            tooltipPanel.SetActive(true);
        }
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}
