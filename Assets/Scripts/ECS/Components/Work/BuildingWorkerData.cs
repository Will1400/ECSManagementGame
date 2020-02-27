using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct BuildingWorkerData : IComponentData
{
    public int MaxWorkers;
    public int ActiveWorkers;
    public int CurrentWorkers;
    public float3 WorkPosition;
}
