using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct NavAgentRequestingPath : IComponentData
{
    public float3 StartPosition;
    public float3 EndPosition;
}
