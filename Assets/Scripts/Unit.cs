using System;
using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;
using System.Linq;

public class Unit : MonoBehaviour
{

    #region Actions

    private MoveAction moveAction;
    private BaseAction[] baseActionArray;

    #endregion

    #region Health

    [SerializeField] private HealthSystem healthSystem;

    #endregion

    #region EventHandlers

    public static event EventHandler OnAnyActionPointsChanged;
    
    #endregion

    #region ActionPoints

    private const int ACTION_POINTS_MAX = 5;
    [SerializeField] private int actionPoints = ACTION_POINTS_MAX;

    #endregion

    [FormerlySerializedAs("unitID")] public int teamID;

    [SerializeField] private bool isEnemy;
    
    [SerializeField] private int maxActionPoints = 5;

    private BaseAction defaultCombatAction;

    [SerializeField] private GameObject damageTextPrefab; // Unity Inspector'da atanacak

    private List<StatusEffect> activeEffects = new List<StatusEffect>();

    public HealthSystem HealthSystem { get; private set; }

    private void Awake()
    {
        HealthSystem = GetComponent<HealthSystem>();
        moveAction = GetComponent<MoveAction>();
        baseActionArray = GetComponents<BaseAction>();
        
        // Event'i dinle
        healthSystem.OnDamageTaken += HealthSystem_OnDamageTaken;
        
        // Varsayılan combat action'ı belirle
        foreach (BaseAction action in baseActionArray)
        {
            if (action.IsDefaultCombatAction())
            {
                if (action is BowRangeAction && !IsEnemy())  // Archer için
                {
                    defaultCombatAction = action;
                    break;
                }
                else if (action is MeleeAction && !IsEnemy())  // Knight için
                {
                    defaultCombatAction = action;
                    break;
                }
                else if (action is BerserkerMeleeAction && !IsEnemy()) // Berserker için
                {
                    defaultCombatAction = action;
                    break;
                }
                else if (action is BasicStabRogue && !IsEnemy()) // Rogue için
                {
                    defaultCombatAction = action;
                    break;
                }
                else if (action is BasicSpells && !IsEnemy()) // Mage için
                {
                    defaultCombatAction = action;
                    break;
                }
            }
        }
    }

    private void Start()
    {
        TurnSystem.instance.OnTurnChanged += TurnSystem_OnTurnChanged;
        actionPoints = maxActionPoints;
        healthSystem.OnDead += HealthSystem_OnDead;
    }

    #region GetActionField

    public MoveAction GetMoveAction()
    {
        return moveAction;
    }

    public BaseAction[] GetBaseActionArray()
    {
        return baseActionArray;
    }

    #endregion
    
    #region ActionPointManagements

    public bool TrySpendActionPointsToTakeAction(BaseAction baseAction)
    {
        if (CanSpendActionPointsToTakeAction(baseAction))
        {
            SpendActionPoints(baseAction.GetActionPointsCost());
            return true;
        }
        else
        {
            
            return false;
        }
    }
    
    public bool CanSpendActionPointsToTakeAction(BaseAction baseAction)
    {

        return actionPoints >= baseAction.GetActionPointsCost();

    }

    private void SpendActionPoints(int amount)
    {
        actionPoints -= amount;
        OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetActionPoints()
    {
        return actionPoints;
    }

    public void ResetActionPoints()
    {
        actionPoints = maxActionPoints;

        Debug.Log("Action Points Reset");
    }

    public BaseAction[] GetActions()
    {
        return GetComponents<BaseAction>();
    }

    #endregion

    #region EventHandlers

    private void TurnSystem_OnTurnChanged(object sender, EventArgs e)
    {
        if ((IsEnemy() && !TurnSystem.instance.IsPlayerTurn()) || (!IsEnemy() && TurnSystem.instance.IsPlayerTurn()))
        {
            actionPoints = ACTION_POINTS_MAX;
            OnAnyActionPointsChanged?.Invoke(this, EventArgs.Empty);
        }
        
        // Her tur başında aktif efektleri uygula
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            // Önce damage text'i göster
            UIManager.Instance.ShowDamageText(
                transform.position + Vector3.up * 0.5f,
                activeEffects[i].damagePerTurn,
                activeEffects[i].GetDamageColor()
            );

            // Hasar ver (text göstermeden)
            healthSystem.DamageWithoutText(activeEffects[i].damagePerTurn);
            
            // Efekti işle
            activeEffects[i].ProcessTurnStart();
            
            if (activeEffects[i].IsFinished)
            {
                activeEffects.RemoveAt(i);
            }
        }
    }

    private void HealthSystem_OnDead(object sender, EventArgs e)
    {
        Destroy(gameObject);
    }

    private void HealthSystem_OnDamageTaken(object sender, HealthSystem.OnDamageTakenEventArgs e)
    {
        // Artık burada bir şey yapmamıza gerek yok
        // DamageTextAnimation kendi eventini dinliyor
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
        {
            healthSystem.OnDamageTaken -= HealthSystem_OnDamageTaken;
        }
    }

    #endregion

    public bool IsEnemy()
    {
        return isEnemy;
    }

    public void Damage(int damageAmount)
    {
        // Sadece HealthSystem'e hasar ver, geri kalanını event halledecek
        healthSystem.Damage(damageAmount);
    }

    public Vector3 GetUnitWorldPosition()
    {
       return transform.position;     
    }
    
    public BaseAction GetDefaultCombatAction()
    {
        return defaultCombatAction;
    }

    public void AddStatusEffect(StatusEffect effect)
    {
        activeEffects.Add(effect);
    }

    public bool HasEffect<T>() where T : StatusEffect
    {
        return activeEffects.Any(e => e is T);
    }
}
