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
using Unity.Transforms;
using UnityEngine;

[UpdateAfter(typeof(NavMeshSystem))]
public class PathfindingSystem : SystemBase
{
    NativeHashMap<int, Entity> queuedEntities;
    Dictionary<int, float3[]> readyPaths;

    protected override void OnCreate()
    {
        queuedEntities = new NativeHashMap<int, Entity>(10, Allocator.Persistent);
        readyPaths = new Dictionary<int, float3[]>();
        NavMeshQuerySystem.RegisterPathResolvedCallbackStatic(OnPathRequestCompleted);
        NavMeshQuerySystem.RegisterPathFailedCallbackStatic(OnPathRequestFailed);
    }

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref NavAgent navAgent, ref NavAgentRequestingPath agentRequestingPath) =>
        {
            if (navAgent.Status != AgentStatus.PathQueued && !queuedEntities.ContainsKey(entity.Index))
            {
                queuedEntities.Add(entity.Index, entity);
                navAgent.Status = AgentStatus.PathQueued;

                if (math.all(agentRequestingPath.StartPosition == float3.zero))
                    agentRequestingPath.StartPosition = EntityManager.GetComponentData<Translation>(entity).Value;

                NavMeshQuerySystem.instance.RequestPath(entity.Index, agentRequestingPath.StartPosition, agentRequestingPath.EndPosition);
            }
        }).WithoutBurst().Run();

        for (int i = 0; i < readyPaths.Keys.Count; i++)
        {
            var keyValuePair = readyPaths.ElementAt(i);
            var entity = queuedEntities[keyValuePair.Key];

            var agent = EntityManager.GetComponentData<NavAgent>(entity);
            agent.Status = AgentStatus.Moving;
            agent.TotalWaypoints = keyValuePair.Value.Length;
            agent.CurrentWaypointIndex = 0;

            EntityManager.AddBuffer<NavAgentPathPointElement>(entity);

            var buffer = EntityManager.GetBuffer<NavAgentPathPointElement>(entity);

            if (buffer.Length > 0)
                buffer.Clear();

            for (int j = 0; j < keyValuePair.Value.Length; j++)
            {
                buffer.Add(keyValuePair.Value[j]);
            }

            EntityManager.AddComponent<NavAgentHasPathTag>(entity);
            EntityManager.SetComponentData(entity, agent);
            EntityManager.RemoveComponent<NavAgentRequestingPath>(entity);
            EntityManager.RemoveComponent<HasArrivedAtDestinationTag>(entity);

            queuedEntities.Remove(keyValuePair.Key);
        }
        readyPaths.Clear();
    }

    void OnPathRequestFailed(int id, PathfindingFailedReason reason)
    {
        var entity = queuedEntities[id];
        float3 endPosition = EntityManager.GetComponentData<NavAgentRequestingPath>(entity).EndPosition;

        if (math.all(endPosition == float3.zero))
        {
            queuedEntities.Remove(id);
        }
        else
        {
            readyPaths.Add(id, new float3[] { endPosition });
        }
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
