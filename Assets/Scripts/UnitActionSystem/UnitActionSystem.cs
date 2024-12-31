using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
// ReSharper disable All

public class UnitActionSystem : MonoBehaviour
{
   public static UnitActionSystem Instance {get; private set;}
   
   [SerializeField] private Unit selectedUnit;
   private BaseAction selectedAction;
   
   private bool isBusy;
   
   private MovementRangeVisualizer currentRangeVisualizer;
   private MoveAction currentMoveAction;
   
   #region EventHandlers

   public event EventHandler OnSelectedUnitChanged;
   public event EventHandler OnSelectedActionChanged;
   public event EventHandler <bool> OnBusyChanged;
   public event EventHandler OnActionStarted;
   

   #endregion
   
   #region LayerMasks

   [SerializeField] private LayerMask unitLayerMask;
   #endregion
 
  
   private void Awake()
   {


      if (Instance != null)
      {
         Debug.LogError("More than one instance of UnitActionSystem found!"+transform+" - " + Instance);
         Destroy(gameObject);
         return;
      }
        
        Instance = this;
   }


   private void Start()
   {
      // Eğer seçili unit yoksa veya sahnede değilse
      if (selectedUnit == null || !selectedUnit.gameObject.activeInHierarchy)
      {
         Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
         List<Unit> allyUnits = new List<Unit>();
         
         foreach (Unit unit in allUnits)
         {
            if (!unit.IsEnemy() && unit.gameObject.activeInHierarchy)
            {
               allyUnits.Add(unit);
            }
         }
         
         if (allyUnits.Count > 0)
         {
            selectedUnit = allyUnits[UnityEngine.Random.Range(0, allyUnits.Count)];
            SetSelectedUnit(selectedUnit);
         }
         else
         {
            Debug.LogWarning("No active ally units found in scene!");
         }
      }

      TurnSystem.instance.OnTurnChanged += TurnSystem_OnTurnChanged;

      // AimArrowAction event'lerini dinle
      Unit[] unitsWithAimArrow = FindObjectsByType<Unit>(FindObjectsSortMode.None);
      foreach (Unit unit in unitsWithAimArrow)
      {
         AimArrowAction aimArrowAction = unit.GetComponent<AimArrowAction>();
         if (aimArrowAction != null)
         {
            aimArrowAction.OnShootAnimStarted += AimArrowAction_OnShootAnimStarted;
            aimArrowAction.OnShootCompleted += AimArrowAction_OnShootCompleted;
         }
      }
   }

   private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
   {
      // Turn değiştiğinde busy state'i temizle
      ClearBusy();
   }

   private void OnDestroy()
   {
      // Event subscription'ı temizle
      if (TurnSystem.instance != null)
      {
         TurnSystem.instance.OnTurnChanged -= TurnSystem_OnTurnChanged;
      }
   }

   private void Update()
   {
      if (isBusy || !TurnSystem.instance.IsPlayerTurn() || EventSystem.current.IsPointerOverGameObject())
      {
         return;
      }

      Vector3 mousePosition = MouseWorld.GetMouseWorldPosition();

      // Move Action path gösterimi
      if (selectedAction is MoveAction moveAction)
      {
         moveAction.ShowPath(mousePosition);
         
         if (currentRangeVisualizer == null)
         {
            currentRangeVisualizer = selectedUnit.GetComponent<MovementRangeVisualizer>();
            if (currentRangeVisualizer == null)
            {
               currentRangeVisualizer = selectedUnit.gameObject.AddComponent<MovementRangeVisualizer>();
            }
         }
         
         float maxRange = moveAction.GetMaxMovementPoints() / moveAction.GetMovementCostPerUnit();
         currentRangeVisualizer.ShowRange(maxRange);
      }
      // Melee Action için ShowPath'i kaldırdık çünkü artık NavMesh kullanıyoruz
      
      if (Input.GetMouseButtonDown(0))
      {
         if (TryHandleUnitSelection())
         {
            return;
         }
         HandleSelectedAction();
      }
   }

   #region UnitSelection

   private void HandleSelectedAction()
   {
      if (selectedUnit == null || selectedAction == null)
      {
         return;
      }

      // Eğer MeleeAction ise ve yapılamıyorsa, busy state'e geçme
      if (selectedAction is MeleeAction meleeAction)
      {
         // Mouse pozisyonundaki hedefi bul
         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
         if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
         {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit targetUnit) && targetUnit.IsEnemy())
            {
               // Hedefin geçerli olup olmadığını kontrol et
               List<Unit> validTargets = meleeAction.GetValidTargetListWithSphere(
                   meleeAction.GetAttackRange() + 
                   selectedUnit.GetMoveAction().GetMaxMovementPoints() / 
                   selectedUnit.GetMoveAction().GetMovementCostPerUnit()
               );

               if (!validTargets.Contains(targetUnit))
               {
                   return; // Hedef menzil dışında
               }

               float distanceToTarget = Vector3.Distance(selectedUnit.transform.position, targetUnit.transform.position);
               bool canExecuteAction = distanceToTarget <= meleeAction.GetAttackRange() || 
                                     selectedUnit.GetMoveAction().GetCurrentMovementPoints() > 0;

               if (!canExecuteAction)
               {
                   return;
               }

               if (!selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
               {
                   return;
               }

               SetBusy();
               selectedAction.TakeAction(targetUnit.transform.position, ClearBusy);
               OnActionStarted?.Invoke(this, EventArgs.Empty);
               return;
            }
         }
         return; // Geçerli hedef bulunamadı
      }

      // Diğer action'lar için normal işlem
      if (!selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
      {
         return;
      }

      SetBusy();
      selectedAction.TakeAction(MouseWorld.GetMouseWorldPosition(), ClearBusy);
      OnActionStarted?.Invoke(this, EventArgs.Empty);
   }
   
   private bool TryHandleUnitSelection()
   {
      if (Input.GetMouseButtonDown(0))
      {
         // Eğer Rest Ally action seçiliyse unit seçimine izin verme
         if (selectedAction is RestActionPoints)
         {
            return false;
         }

         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
         if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
         {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit unit))
            {
               if (unit == selectedUnit)
               {
                  return false;
               }

               if (unit.IsEnemy())
               {
                  return false;
               }
               
               SetSelectedUnit(unit);
               return true;
            }
         }
      }
      
      return false;
   }


   
   
   #endregion

   #region SetBusyManagement

   public void SetBusy()
   {
      isBusy = true;
      OnBusyChanged?.Invoke(this, isBusy);
   }

   public void ClearBusy()
   {
      isBusy = false;
      OnBusyChanged?.Invoke(this, isBusy);
   }

   private void AimArrowAction_OnShootAnimStarted(object sender, EventArgs e)
   {
      SetBusy();
   }

   private void AimArrowAction_OnShootCompleted(object sender, EventArgs e)
   {
      ClearBusy();
   }

   #endregion
   
   #region GetUnitAndAction

   public Unit GetSelectedUnit()
   {
      return selectedUnit;
   }
   
   public BaseAction GetSelectedAction()
   {
      return selectedAction;
   }

   #endregion

   #region SetUnitAndAction

   private void SetSelectedUnit(Unit unit)
   {
      // Önceki unit'in görsellerini temizle
      if (selectedUnit != null)
      {
         if (currentMoveAction != null)
         {
            currentMoveAction.HidePath();
         }
         if (currentRangeVisualizer != null)
         {
            currentRangeVisualizer.HideRange();
         }
      }

      // Yeni unit'i seç
      selectedUnit = unit;
      SetSelectedAction(unit.GetMoveAction());
      
      OnSelectedUnitChanged?.Invoke(this, EventArgs.Empty);
   }

   
  

   public void SetSelectedAction(BaseAction baseAction)
   {
      selectedAction = baseAction;

      if (selectedAction is MoveAction moveAction)
      {
         currentMoveAction = moveAction;
         
         // Range visualizer'ı kontrol et ve yoksa ekle
         currentRangeVisualizer = selectedUnit.GetComponent<MovementRangeVisualizer>();
         if (currentRangeVisualizer == null)
         {
            currentRangeVisualizer = selectedUnit.gameObject.AddComponent<MovementRangeVisualizer>();
         }

         float maxRange = moveAction.GetMaxMovementPoints() / moveAction.GetMovementCostPerUnit();
         currentRangeVisualizer.ShowRange(maxRange);
      }
      else
      {
         // Başka bir aksiyon seçildiğinde gösterimleri kapat
         if (currentMoveAction != null)
         {
            currentMoveAction.HidePath();
         }
         if (currentRangeVisualizer != null)
         {
            currentRangeVisualizer.HideRange();
         }
      }

      OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
   }

   #endregion
   
   
}
