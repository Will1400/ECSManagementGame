using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct ResourceInStorage : IComponentData
{
    public Entity StorageEntity;
    public ResourceData ResourceData;

    //public int Storage
    
    public float3 StorageAreaStartPosition;
    public float3 StorageAreaEndPosition;
}
