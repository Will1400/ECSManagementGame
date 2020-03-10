using UnityEngine;
using System.Collections;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct NavMeshSurface : IComponentData
{
    public int Area;
}
