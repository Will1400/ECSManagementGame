using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct NeedsWorkers : IComponentData
{
    public int WorkersNeeded;
    public float3 WorkPosition;
}
