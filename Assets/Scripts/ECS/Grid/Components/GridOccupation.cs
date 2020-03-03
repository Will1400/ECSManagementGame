using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;

public struct GridOccupation : IComponentData
{
    public int2 Start;
    public int2 End;
}
