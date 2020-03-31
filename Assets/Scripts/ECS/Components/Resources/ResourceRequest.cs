using UnityEngine;
using System.Collections;
using Unity.Entities;

public struct ResourceRequest : IComponentData
{
    public Entity RequestingEntity;
    public ResourceType ResourceType;
    public int Amount;
}
