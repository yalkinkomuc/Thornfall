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

      // Mouse pozisyonunda düşman var mı kontrol et
      bool isOverEnemy = false;
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
      {
         if (raycastHit.transform.TryGetComponent<Unit>(out Unit targetUnit) && targetUnit.IsEnemy())
         {
            isOverEnemy = true;
            // Düşman üzerindeyken varsayılan combat action'ı seç
            if (selectedUnit != null && selectedUnit.GetDefaultCombatAction() != null)
            {
               SetSelectedAction(selectedUnit.GetDefaultCombatAction());
            }
         }
      }

      // Düşman üzerinde değilse ve combat action seçiliyse MoveAction'a geri dön
      if (!isOverEnemy && selectedAction != null && selectedAction.IsCombatAction())
      {
         SetSelectedAction(selectedUnit.GetMoveAction());
      }

      // Move Action path gösterimi
      if (selectedAction is MoveAction moveAction)
      {
         moveAction.ShowPath(mousePosition);
      }

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

      // Eğer MoveAction seçiliyken düşmana tıklandıysa
      if (selectedAction is MoveAction)
      {
         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
         if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
         {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit targetUnit) && targetUnit.IsEnemy())
            {
               BaseAction combatAction = selectedUnit.GetDefaultCombatAction();
               if (combatAction != null)
               {
                  // Önce hareket et, sonra saldır
                  MoveAction moveAction = selectedUnit.GetMoveAction();
                  Vector3 targetPosition = targetUnit.transform.position;

                  SetBusy();
                  moveAction.TakeAction(targetPosition, () => {
                     // Hareket bittikten sonra combat action'ı uygula
                     if (selectedUnit.TrySpendActionPointsToTakeAction(combatAction))
                     {
                        combatAction.TakeAction(targetPosition, ClearBusy);
                     }
                     else
                     {
                        ClearBusy();
                     }
                  });
                  OnActionStarted?.Invoke(this, EventArgs.Empty);
                  return;
               }
            }
         }
      }

      // Normal action handling
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
        

         // Gerçek hareket mesafesini kullan
         float actualRange = moveAction.GetMaxMovementPoints() / moveAction.GetMovementCostPerUnit();
         
      }
      else
      {
         // Başka bir aksiyon seçildiğinde gösterimleri kapat
         if (currentMoveAction != null)
         {
            currentMoveAction.HidePath();
         }
        
      }

      OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
   }

   #endregion
   
   
}
