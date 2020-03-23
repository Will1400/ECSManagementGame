using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct ResourceData : IComponentData
{
    public int Amount;
    public ResourceType ResourceType;
}
