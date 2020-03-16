﻿using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct NavMeshSurface : IComponentData
{
    public int Area;
    public float3 Size;
}