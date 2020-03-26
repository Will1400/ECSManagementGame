using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct MovingToPickupResource : IComponentData
{
    public Entity ResourceEntity;
}
