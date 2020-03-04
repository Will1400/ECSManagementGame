using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;

public struct GridOccupation : IComponentData, IEquatable<GridOccupation>
{
    public int2 Start;
    public int2 End;

    public bool Equals(GridOccupation other)
    {
        return Start.Equals(other.Start) &&
               End.Equals(other.End);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
