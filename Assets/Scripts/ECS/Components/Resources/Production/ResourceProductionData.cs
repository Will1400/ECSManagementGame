﻿using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ResourceProductionData : IComponentData
{
    public ResourceType ResourceType;
    public int AmountPerProduction;

    public float ProductionTime;
    public float ProductionTimeRemaining;

    public float3 SpawnPointOffset;
}
