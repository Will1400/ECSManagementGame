using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct ResourceRequestData : IComponentData
{
    public Entity RequestingEntity;
    public float3 RequestingEntityPosition;
    public ResourceType ResourceType;
    public int Amount;
}
