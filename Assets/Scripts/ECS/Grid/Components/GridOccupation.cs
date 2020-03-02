using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

public struct GridOccupation : IComponentData
{
    public GridPosition StartX;
    public GridPosition EndX;
    public GridPosition StartY;
    public GridPosition EndY;
}
