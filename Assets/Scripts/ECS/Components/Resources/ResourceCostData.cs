using UnityEngine;
using System.Collections;
using Unity.Entities;
using System;

[Serializable]
[GenerateAuthoringComponent]
public struct ResourceCostData : IComponentData
{
    public ResourceType ResourceType;
    public int Amount;
}
