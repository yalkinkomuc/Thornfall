using System;
using UnityEngine;

public class TurnSystem : MonoBehaviour
{
    private int currentTurn =1;
    
    public static TurnSystem instance { get; private set; }

    public event EventHandler OnTurnChanged;

    private bool isPlayerTurn = true;
    private void Awake()
    {
        
        if (instance != null)
        {
            Debug.LogError("More than one instance of TurnSystem found!"+transform+" - " + instance);
            Destroy(gameObject);
            return;
        }
        
        instance = this;
    }

    public void NextTurn()
    {
        currentTurn++;
        isPlayerTurn = !isPlayerTurn;
        
        // Turn değiştiğinde tüm attack state'leri resetlensin
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (Unit unit in allUnits)
        {
            var meleeActions = unit.GetComponents<BaseMeleeAction>();
            foreach (var meleeAction in meleeActions)
            {
                meleeAction.isAttacking = false;
            }
        }
        
        OnTurnChanged?.Invoke(this, EventArgs.Empty);
    }

    public int GetTurnNumber()
    {
        return currentTurn;
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }
}
