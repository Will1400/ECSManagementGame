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

[UpdateAfter(typeof(NavMeshSystem))]
public class PathfindingSystem : ComponentSystem
{
    NativeHashMap<int, Entity> queuedEntities;
    Dictionary<int, float3[]> readyPaths;

    EntityQuery NewRequestedPaths;

    protected override void OnCreate()
    {
        queuedEntities = new NativeHashMap<int, Entity>(10, Allocator.Persistent);
        readyPaths = new Dictionary<int, float3[]>();
        NavMeshQuerySystem.RegisterPathResolvedCallbackStatic(OnPathRequestCompleted);
        NavMeshQuerySystem.RegisterPathFailedCallbackStatic(OnPathRequestFailed);

        NewRequestedPaths = Entities.WithAll<NavAgent, NavAgentRequestingPath>()
                                    .ToEntityQuery();
    }

    protected override void OnUpdate()
    {
        Entities.With(NewRequestedPaths).ForEach((Entity entity, ref NavAgent navAgent, ref NavAgentRequestingPath agentRequestingPath) =>
        {
            if (navAgent.Status != AgentStatus.PathQueued)
            {
                queuedEntities.Add(entity.Index, entity);
                navAgent.Status = AgentStatus.PathQueued;
                NavMeshQuerySystem.RequestPathStatic(entity.Index, agentRequestingPath.StartPosition, agentRequestingPath.EndPosition);
            }
        });

        for (int i = 0; i < readyPaths.Keys.Count; i++)
        {
            var keyValuePair = readyPaths.ElementAt(i);
            var entity = queuedEntities[keyValuePair.Key];

            var agent = EntityManager.GetComponentData<NavAgent>(entity);
            agent.Status = AgentStatus.Moving;
            agent.TotalWaypoints = keyValuePair.Value.Length;

            EntityManager.AddBuffer<Float3BufferElement>(entity);

            var buffer = EntityManager.GetBuffer<Float3BufferElement>(entity);
            for (int j = 0; j < keyValuePair.Value.Length; j++)
            {
                buffer.Add(keyValuePair.Value[j]);
            }

            EntityManager.SetComponentData(entity, agent);
            EntityManager.RemoveComponent<NavAgentRequestingPath>(entity);

            queuedEntities.Remove(keyValuePair.Key);
        }
        readyPaths.Clear();
    }

    void OnPathRequestFailed(int id, PathfindingFailedReason reason)
    {
        var entity = queuedEntities[id];
        var agent = EntityManager.GetComponentData<NavAgent>(entity);
        agent.Status = AgentStatus.Idle;
        EntityManager.SetComponentData(entity, agent);
        queuedEntities.Remove(id);
    }

    void OnPathRequestCompleted(int id, Vector3[] points)
    {
        var buffer = new float3[points.Length];

        for (int i = 0; i < points.Length; i++)
        {
            buffer[i] = points[i];
        }

        readyPaths.Add(id, buffer);
    }

    protected override void OnDestroy()
    {
        queuedEntities.Dispose();
    }
}
