using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]

public struct MoveTowardsComponent : IComponentData
{
    public float3 TargetPosition;
}
