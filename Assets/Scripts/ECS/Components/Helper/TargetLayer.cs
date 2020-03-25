using UnityEngine;
using System.Collections;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct TargetLayer : IComponentData
{
    public uint Value;
}
