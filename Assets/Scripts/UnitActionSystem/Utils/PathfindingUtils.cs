using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class PathfindingUtils : MonoBehaviour, IPathfinding
{
    private MoveAction moveAction;
    private Unit unit;

    private void Awake()
    {
        moveAction = GetComponent<MoveAction>();
        unit = GetComponent<Unit>();
    }

    public List<Unit> GetValidTargetListWithPath(float radius, float maxMoveRange)
    {
        List<Unit> validTargets = new List<Unit>();
        Unit[] allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);

        foreach (Unit potentialTarget in allUnits)
        {
            if (!potentialTarget.IsEnemy()) continue;

            NavMeshPath path = new NavMeshPath();
            if (NavMesh.CalculatePath(unit.GetUnitWorldPosition(), potentialTarget.GetUnitWorldPosition(), NavMesh.AllAreas, path))
            {
                float pathLength = CalculatePathLength(path.corners);
                if (pathLength <= maxMoveRange + radius)
                {
                    validTargets.Add(potentialTarget);
                }
            }
        }

        return validTargets;
    }

    public float CalculatePathLength(Vector3[] pathPoints)
    {
        float length = 0;
        for (int i = 0; i < pathPoints.Length - 1; i++)
        {
            length += Vector3.Distance(pathPoints[i], pathPoints[i + 1]);
        }
        return length;
    }
} 