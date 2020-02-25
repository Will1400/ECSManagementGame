using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class SpawnSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
        }
    }
}
