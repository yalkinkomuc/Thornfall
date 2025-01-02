using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.AI;
using System.Linq;
// ReSharper disable All

public class UnitActionSystem : MonoBehaviour
{
   public static UnitActionSystem Instance {get; private set;}
   
   [SerializeField] private Unit selectedUnit;
   private BaseAction selectedAction;
   
   private bool isBusy;
   
   
   private MoveAction currentMoveAction;
   
   private bool isActionAutoSelected = false;  // Action otomatik mi seçildi?
   
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

      // Sadece MoveAction seçiliyken veya otomatik seçilmiş action varken kontrol yap
      if (selectedAction is MoveAction || isActionAutoSelected)
      {
         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
         bool isOverEnemy = false;

         if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
         {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit targetUnit) && targetUnit.IsEnemy())
            {
               isOverEnemy = true;
               // Düşman üzerindeyken varsayılan combat action'ı seç
               if (selectedUnit != null && selectedUnit.GetDefaultCombatAction() != null)
               {
                  isActionAutoSelected = true;
                  SetSelectedAction(selectedUnit.GetDefaultCombatAction());
               }
            }
         }

         // Sadece otomatik seçilmiş action varken MoveAction'a geri dön
         if (!isOverEnemy && isActionAutoSelected)
         {
            SetSelectedAction(selectedUnit.GetMoveAction());
            isActionAutoSelected = false;
         }
      }

      // Move Action path gösterimi
      if (selectedAction is MoveAction moveAction)
      {
         moveAction.ShowPath(MouseWorld.GetMouseWorldPosition());
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

      // Eğer zaten bir hedef birimi varsa, yeni bir aksiyon alımına izin verme
      if (selectedAction is MeleeAction meleeAction && meleeAction.isAttacking)
      {
         return;
      }

      if (selectedAction is BowRangeAction bowRangeAction && bowRangeAction.isAttacking)
      {
         return;
      }

      if (selectedAction is HeavyAttackAction heavyAttackAction && heavyAttackAction.isAttacking)
      {
         return;
      }

      if (selectedAction is AimArrowAction aimArrowAction && aimArrowAction.isAttacking)
      {
         return;
      }

      // Melee veya Heavy Attack için hedef kontrolü yap
      if (selectedAction is MeleeAction || selectedAction is HeavyAttackAction)
      {
         Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
         if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
         {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit targetUnit) && targetUnit.IsEnemy())
            {
               // Action point kontrolünü kaldır
               SetBusy();
               selectedAction.TakeAction(MouseWorld.GetMouseWorldPosition(), ClearBusy);
               OnActionStarted?.Invoke(this, EventArgs.Empty);
            }
         }
         return;
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

   private void HandleMeleeAction()
   {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue, unitLayerMask))
      {
         if (raycastHit.transform.TryGetComponent<Unit>(out Unit targetUnit) && targetUnit.IsEnemy())
         {
            if (selectedUnit.TrySpendActionPointsToTakeAction(selectedAction))
            {
               SetBusy();
               selectedAction.TakeAction(MouseWorld.GetMouseWorldPosition(), ClearBusy);
               OnActionStarted?.Invoke(this, EventArgs.Empty);
            }
         }
      }
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

   private Vector3 FindBestShootPosition(Unit targetUnit, float maxRange)
   {
      const int SAMPLE_COUNT = 8; // Çevrede kontrol edilecek nokta sayısı
      float currentRange = maxRange * 0.7f; // Optimal mesafe (max menzilden biraz kısa)
      
      // Önce mevcut pozisyondan görüş var mı kontrol et
      Vector3 directionToTarget = (targetUnit.transform.position - selectedUnit.transform.position).normalized;
      if (!Physics.Raycast(selectedUnit.transform.position, directionToTarget, out RaycastHit initialHit, maxRange) ||
          initialHit.transform.GetComponent<Unit>() == targetUnit)
      {
          return selectedUnit.transform.position; // Mevcut pozisyon uygunsa hareket etme
      }

      // Hedefin etrafında noktalar kontrol et
      List<(Vector3 position, float distance)> validPositions = new List<(Vector3, float)>();
      
      for (int i = 0; i < SAMPLE_COUNT; i++)
      {
          float angle = i * (360f / SAMPLE_COUNT);
          Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
          Vector3 potentialPosition = targetUnit.transform.position + direction * currentRange;

          // NavMesh üzerinde geçerli bir nokta bul
          if (NavMesh.SamplePosition(potentialPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
          {
              // Hedef görüş hattında mı kontrol et
              directionToTarget = (targetUnit.transform.position - hit.position).normalized;
              if (!Physics.Raycast(hit.position, directionToTarget, out RaycastHit raycastHit, currentRange) ||
                  raycastHit.transform.GetComponent<Unit>() == targetUnit)
              {
                  // Geçerli pozisyonu ve mevcut pozisyona olan mesafeyi kaydet
                  float distanceFromCurrent = Vector3.Distance(hit.position, selectedUnit.transform.position);
                  validPositions.Add((hit.position, distanceFromCurrent));
              }
          }
      }

      // Eğer geçerli pozisyon bulunamadıysa, daha geniş bir alanda ara
      if (validPositions.Count == 0)
      {
          currentRange = maxRange; // Maksimum menzili kullan
          for (int i = 0; i < SAMPLE_COUNT; i++)
          {
              float angle = i * (360f / SAMPLE_COUNT);
              Vector3 direction = Quaternion.Euler(0, angle, 0) * Vector3.forward;
              Vector3 potentialPosition = targetUnit.transform.position + direction * currentRange;

              if (NavMesh.SamplePosition(potentialPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
              {
                  directionToTarget = (targetUnit.transform.position - hit.position).normalized;
                  if (!Physics.Raycast(hit.position, directionToTarget, out RaycastHit raycastHit, currentRange) ||
                      raycastHit.transform.GetComponent<Unit>() == targetUnit)
                  {
                      float distanceFromCurrent = Vector3.Distance(hit.position, selectedUnit.transform.position);
                      validPositions.Add((hit.position, distanceFromCurrent));
                  }
              }
          }
      }

      // En yakın geçerli pozisyonu seç
      if (validPositions.Count > 0)
      {
          return validPositions.OrderBy(x => x.distance).First().position;
      }

      return Vector3.zero; // Hiçbir uygun pozisyon bulunamadı
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

   // private void AimArrowAction_OnShootAnimStarted(object sender, EventArgs e)
   // {
   //    SetBusy();
   // }
   //
   // private void AimArrowAction_OnShootCompleted(object sender, EventArgs e)
   // {
   //    ClearBusy();
   // }

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
      // UI'dan manuel seçim yapılıyorsa flag'i resetle
      if (EventSystem.current.IsPointerOverGameObject())
      {
         isActionAutoSelected = false;
      }

      selectedAction = baseAction;
      
      if (selectedAction is MoveAction moveAction)
      {
         currentMoveAction = moveAction;
      }
      else
      {
         if (currentMoveAction != null)
         {
            currentMoveAction.HidePath();
         }
      }

      OnSelectedActionChanged?.Invoke(this, EventArgs.Empty);
   }

   #endregion
   
   
}
