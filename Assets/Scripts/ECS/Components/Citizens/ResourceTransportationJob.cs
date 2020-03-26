using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct ResourceTransportJobData : IComponentData
{
    public Entity ResourceEntity;
    public Entity DestinationEntity;
    public float3 DestinationPosition;

}
