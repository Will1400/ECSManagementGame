using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct NavAgentRequestingPath : IComponentData
{
    public float3 StartPosition;
    public float3 EndPosition;
}
