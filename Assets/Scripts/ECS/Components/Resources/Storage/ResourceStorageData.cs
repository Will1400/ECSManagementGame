using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ResourceStorageData : IComponentData
{
    public int MaxCapacity;
    public int UsedCapacity;
    public float3 StoragePosition;
}
