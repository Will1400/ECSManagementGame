using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using System;
using System.Runtime.Serialization;

[Serializable]
[GenerateAuthoringComponent]
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
        int hash = 13;
        hash = (hash * 7) + Start.x;
        hash = (hash * 7) + Start.y;
        hash = (hash * 7) + End.x;
        hash = (hash * 7) + End.y;
        return hash;
    }
}
