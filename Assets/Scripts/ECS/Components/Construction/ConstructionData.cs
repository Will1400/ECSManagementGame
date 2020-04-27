using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[GenerateAuthoringComponent]
public struct ConstructionData : IComponentData
{
    public float TotalConstructionTime;
    public float RemainingConstructionTime;
    public NativeString128 FinishedPrefabName;
}
