using UnityEngine;
using System.Collections;
using Unity.Entities;
using System;

[Serializable]
[GenerateAuthoringComponent]
public struct ResourceCost : IComponentData
{
    public ResourceType ResourceType;
    public int Amount;
}
