using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class BaseAction : MonoBehaviour, ITargetVisualAction
{
    protected Unit unit;
    protected bool isActive;
    protected Action onActionComplete;

    [SerializeField] protected LayerMask whatIsUnit;
    [FormerlySerializedAs("isCombatAction")] [SerializeField] protected bool isDefaultCombatAction = false;
    
    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
    }

    public abstract string GetActionName();

    public virtual int GetActionPointsCost()
    {
        return 0;
    }

    public abstract void TakeAction(Vector3 worldPosition,Action onActionComplete);

    protected void ActionStart(Action onActionComplete)
    {
        isActive = true;
        this.onActionComplete = onActionComplete;
    }

    protected void ActionComplete()
    {
        if (!isActive) return; // Zaten aktif değilse dön

        isActive = false;
        if (onActionComplete != null)
        {
            var callback = onActionComplete;
            onActionComplete = null;
            callback();
        }
    }


    public virtual Unit GetValidTarget(float attackRange)
    {
        Vector3 mouseWorldPosition = MouseWorld.GetMouseWorldPosition();
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(cameraRay, out RaycastHit cameraHit, float.MaxValue))
        {
            Unit targetUnit = cameraHit.collider.GetComponent<Unit>();
            
            if (targetUnit != null && unit.teamID != targetUnit.teamID) // düşman checkini ve takım kontrolünü burdan yapıyoruz
            {
                // Sabit yükseklikte başlangıç ve hedef noktaları
                Vector3 shootPosition = new Vector3(transform.position.x, 1f, transform.position.z);
                Vector3 targetPosition = new Vector3(targetUnit.transform.position.x, 1f, targetUnit.transform.position.z);
                
                Vector3 directionToTarget = (targetPosition - shootPosition).normalized;
                float distanceToTarget = Vector3.Distance(shootPosition, targetPosition);
                 
                if (distanceToTarget > attackRange) return null; // menzil dışındaysa null döndür

                Debug.DrawRay(shootPosition, directionToTarget * distanceToTarget, Color.red, 1f);
                if (Physics.Raycast(shootPosition, directionToTarget, out RaycastHit lineOfSightHit, distanceToTarget)) // 
                {
                    Unit hitUnit = lineOfSightHit.collider.GetComponent<Unit>();
                    if (hitUnit == targetUnit)
                    {
                        //Debug.Log($"Geçerli hedef bulundu: {targetUnit.name}");
                        return targetUnit;
                    }
                }
                //Debug.Log("Hedef görüş hattında değil veya engel var.");
            }
        }

        return null;
    }



    

    public virtual List<Unit> GetValidTargetListWithSphere(float radius)
    {
        List<Unit> validTargets = new List<Unit>();
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, whatIsUnit);

        foreach (Collider unitHitCollider in colliders)
        {
            if (unitHitCollider == null) continue; // Null kontrolü

            Unit targetUnit = unitHitCollider.GetComponent<Unit>();

            if (targetUnit != null && unit.teamID != targetUnit.teamID)
            {
                // Görüş hattı kontrolü
                Vector3 directionToTarget = (targetUnit.transform.position - transform.position).normalized;
                if (!Physics.Raycast(transform.position, directionToTarget, out RaycastHit hit, radius))
                {
                    // Eğer bir engel yoksa listeye ekle
                    validTargets.Add(targetUnit);
                    //Debug.Log($"Geçerli hedef (Sphere) bulundu: {targetUnit.name}");
                }
                else if (hit.collider.GetComponent<Unit>() == targetUnit)
                {
                    // Eğer ray doğrudan hedefi vuruyorsa yine geçerli
                    validTargets.Add(targetUnit);
                   // Debug.Log($"Görüş hattıyla geçerli hedef bulundu: {targetUnit.name}");
                }
            }
        }

        return validTargets;
    }
       

  
    
    public bool IsDefaultCombatAction() => isDefaultCombatAction;

    public virtual bool ShouldShowTargetVisual(Unit targetUnit)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, float.MaxValue))
        {
            if (raycastHit.transform.TryGetComponent<Unit>(out Unit mouseOverUnit))
            {
                return mouseOverUnit == targetUnit;
            }
        }
        return false;
    }

    protected virtual void Start()
    {
        // Base implementation
    }

    protected virtual void OnDestroy()
    {
        // Base implementation
    }

    public virtual string GetActionDescription()
    {
        return "No description available.";
    }

    protected virtual void Update()
    {
        if (!isActive) return;
        UpdateAction();
    }

    protected virtual void UpdateAction() { }
}
