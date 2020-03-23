using UnityEngine;
using System.Collections;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct ResourceProductionData : IComponentData
{
    public ResourceType ResourceType;
    public int AmountPerProduction;

    public float ProductionTime;
    public float ProductionTimeRemaining;
}
