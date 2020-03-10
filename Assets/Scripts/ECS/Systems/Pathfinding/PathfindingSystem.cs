using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class PathfindingSystem : ComponentSystem
{
    NativeHashMap<int, Entity> queuedEntities;
    NativeList<CompletedPathRequest> readyPaths;

    protected override void OnCreate()
    {
        queuedEntities = new NativeHashMap<int, Entity>(10, Allocator.Persistent);
        readyPaths = new NativeList<CompletedPathRequest>(Allocator.Persistent);
        NavMeshQuerySystem.RegisterPathResolvedCallbackStatic(OnPathRequestCompleted);
    }

    protected override void OnUpdate()
    {


        for (int i = 0; i < readyPaths.Length; i++)
        {
            CompletedPathRequest pathResult = readyPaths[i];
            queuedEntities.Remove(pathResult.Id);
        }
    }

    void OnPathRequestCompleted(int id, Vector3[] points)
    {
        var result = new CompletedPathRequest { Id = id };
        result.Path = new float3[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            result.Path[i] = (points[i]);
        }

        readyPaths.Add(result);
    }

    protected override void OnDestroy()
    {
        queuedEntities.Dispose();
        readyPaths.Dispose();
    }

    [Serializable]
    struct CompletedPathRequest
    {
        public int Id;
        public float3[] Path;
    }
}

