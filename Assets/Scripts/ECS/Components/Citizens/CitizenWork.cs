using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct CitizenWork : IComponentData
{
    public bool IsWorking;
    public Entity WorkPlaceEntity;
    public float3 WorkPosition;
}