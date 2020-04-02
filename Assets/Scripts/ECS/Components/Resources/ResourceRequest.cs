using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct ResourceRequest : IComponentData
{
    public Entity RequestingEntity;
    public float3 RequestingEntityPosition;
    public ResourceType ResourceType;
    public int Amount;
}
