using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct WorkPlaceWorkerData : IComponentData
{
    public bool IsWorkable;
    public int MaxWorkers;
    public int ActiveWorkers;
    public int CurrentWorkers;
    public float3 WorkPosition;
}
