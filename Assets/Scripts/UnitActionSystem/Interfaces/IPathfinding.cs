using System.Collections.Generic;
using UnityEngine;

public interface IPathfinding
{
    List<Unit> GetValidTargetListWithPath(float radius, float maxMoveRange);
    float CalculatePathLength(Vector3[] pathPoints);
} 