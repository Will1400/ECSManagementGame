using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct Float3BufferElement : IBufferElementData
{
    public float3 Value;

    public static implicit operator float3(Float3BufferElement e)
    {
        return e.Value;
    }

    public static implicit operator Float3BufferElement(float3 e)
    {
        return new Float3BufferElement { Value = e };
    }
}
