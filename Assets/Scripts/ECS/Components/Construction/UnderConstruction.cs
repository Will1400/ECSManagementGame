using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[GenerateAuthoringComponent]
public struct UnderConstruction : IComponentData
{
    public float totalConstructionTime;
    public float remainingConstructionTime;
    public NativeString128 finishedPrefabName;
}
