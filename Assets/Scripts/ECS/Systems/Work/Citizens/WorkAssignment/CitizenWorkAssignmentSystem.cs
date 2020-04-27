using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[UpdateInGroup(typeof(WorkAssignmentGroup))]
public class CitizenWorkAssignmentSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery idleCitizensQuery;
    EntityQuery needsWorkersQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        idleCitizensQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen), typeof(IdleTag), typeof(Translation) }
        });

        needsWorkersQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(WorkplaceWorkerData) },
            None = new ComponentType[] { typeof(RemoveWorkplaceTag) }
        });
    }

    protected override void OnUpdate()
    {
        if (idleCitizensQuery.CalculateChunkCount() == 0 || needsWorkersQuery.CalculateChunkCount() == 0)
            return;

        var idleCitizens = idleCitizensQuery.ToEntityArray(Allocator.TempJob);
        var idleCitizensTranslation = idleCitizensQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        EntityCommandBuffer CommandBuffer = bufferSystem.CreateCommandBuffer();

        int citizenIndex = 0;
        Entities.WithNone<RemoveWorkplaceTag>().ForEach((Entity workPlace, ref WorkplaceWorkerData workerData) =>
        {
            if (idleCitizens.Length == 0)
                return;

            if (workerData.CurrentWorkers < workerData.MaxWorkers && workerData.IsWorkable && math.all(workerData.WorkPosition != float3.zero))
            {
                int currentWorkers = workerData.CurrentWorkers;
                for (int i = citizenIndex; i < idleCitizens.Length; i++)
                {
                    if (currentWorkers < workerData.MaxWorkers)
                    {
                        CommandBuffer.AddComponent<CitizenWork>(idleCitizens[i]);
                        CommandBuffer.AddComponent<NavAgentRequestingPath>(idleCitizens[i]);

                        CommandBuffer.SetComponent(idleCitizens[i], new CitizenWork { WorkplaceEntity = workPlace, WorkPosition = workerData.WorkPosition });
                        CommandBuffer.SetComponent(idleCitizens[i], new NavAgentRequestingPath { StartPosition = idleCitizensTranslation[i].Value, EndPosition = workerData.WorkPosition });
                        currentWorkers++;
                        CommandBuffer.RemoveComponent<IdleTag>(idleCitizens[i]);
                        CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(idleCitizens[i]);

                        citizenIndex++;
                    }
                    else
                    {
                        break;
                    }
                }
                workerData.CurrentWorkers = currentWorkers;
            }
        }).Run();

        // Needs to be played back here to remove IdleTag immediately
        CommandBuffer.Playback(EntityManager);
        CommandBuffer.ShouldPlayback = false;

        idleCitizens.Dispose();
        idleCitizensTranslation.Dispose();
    }
}
