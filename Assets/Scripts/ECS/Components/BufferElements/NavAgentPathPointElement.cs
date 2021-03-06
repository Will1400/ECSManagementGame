﻿using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct NavAgentPathPointElement : IBufferElementData
{
    public float3 Value;

    public static implicit operator float3(NavAgentPathPointElement e)
    {
        return e.Value;
    }

    public static implicit operator NavAgentPathPointElement(float3 e)
    {
        return new NavAgentPathPointElement { Value = e };
    }
}
