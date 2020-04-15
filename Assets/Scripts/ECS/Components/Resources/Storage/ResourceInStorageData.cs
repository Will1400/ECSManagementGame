using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct ResourceInStorageData : IComponentData
{
    public Entity StorageEntity;
    public ResourceData ResourceData;

    public float3 StorageAreaStartPosition;
    public float3 StorageAreaEndPosition;
}
