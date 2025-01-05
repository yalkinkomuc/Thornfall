using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SelectedTargetNumber : MonoBehaviour
{
    [SerializeField] private Slider targetSlider;
    [SerializeField] private TextMeshProUGUI selectedCountText;
    [SerializeField] private int maxTargetCount = 3;

    private AimArrowAction currentAimArrowAction;

    private void Start()
    {
        UnitActionSystem.Instance.OnSelectedActionChanged += UnitActionSystem_OnSelectedActionChanged;
        
        targetSlider.minValue = 0;
        targetSlider.maxValue = maxTargetCount;
        targetSlider.wholeNumbers = true;
        
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (UnitActionSystem.Instance != null)
        {
            UnitActionSystem.Instance.OnSelectedActionChanged -= UnitActionSystem_OnSelectedActionChanged;
        }
        UnsubscribeFromCurrentAction();
    }

    private void UnitActionSystem_OnSelectedActionChanged(object sender, System.EventArgs e)
    {
        UnsubscribeFromCurrentAction();

        BaseAction selectedAction = UnitActionSystem.Instance.GetSelectedAction();
        
        if (selectedAction is AimArrowAction aimArrowAction)
        {
            currentAimArrowAction = aimArrowAction;
            currentAimArrowAction.OnTargetListChanged += AimArrowAction_OnTargetListChanged;
            // UI'ı hemen gösterme, ilk hedef seçildiğinde gösterilecek
        }
        else
        {
            currentAimArrowAction = null;
            gameObject.SetActive(false);
        }
    }

    private void UnsubscribeFromCurrentAction()
    {
        if (currentAimArrowAction != null)
        {
            currentAimArrowAction.OnTargetListChanged -= AimArrowAction_OnTargetListChanged;
            currentAimArrowAction = null;
        }
    }

    private void AimArrowAction_OnTargetListChanged(object sender, AimArrowAction.OnTargetListChangedEventArgs e)
    {
        // İlk hedef seçildiğinde UI'ı göster
        if (!gameObject.activeSelf && e.currentTargetCount > 0)
        {
            gameObject.SetActive(true);
        }
        // Son hedef çıkarıldığında UI'ı gizle
        else if (gameObject.activeSelf && e.currentTargetCount == 0)
        {
            gameObject.SetActive(false);
        }

        UpdateUI(e.currentTargetCount);
    }

    private void UpdateUI(int currentCount)
    {
        if (targetSlider != null)
        {
            targetSlider.value = currentCount;
        }

        if (selectedCountText != null)
        {
            if (currentCount == 0)
            {
                selectedCountText.text = "No Target Selected";
            }
            else if (currentCount == 1)
            {
                selectedCountText.text = "1 Target Selected";
            }
            else
            {
                selectedCountText.text = $"{currentCount} Targets Selected";
            }
        }
    }
} 