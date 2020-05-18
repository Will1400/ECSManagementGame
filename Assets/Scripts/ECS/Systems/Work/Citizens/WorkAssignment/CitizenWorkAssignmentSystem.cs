using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Burst;
using JetBrains.Annotations;
using System;
using System.Linq;

[UpdateInGroup(typeof(WorkAssignmentGroup))]
public class CitizenWorkAssignmentSystem : SystemBase
{
    EntityQuery idleCitizensQuery;
    EntityQuery needsWorkersQuery;

    protected override void OnCreate()
    {
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

        var idleCitizenEntitities = idleCitizensQuery.ToEntityArrayAsync(Allocator.TempJob, out JobHandle citizenEntititiesHandle);
        var idleCitizensTranslations = idleCitizensQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out JobHandle citizenTranslationHandle);

        var workplaceEntities = needsWorkersQuery.ToEntityArray(Allocator.TempJob);
        var workplaceWorkerDatas = needsWorkersQuery.ToComponentDataArray<WorkplaceWorkerData>(Allocator.TempJob);

        EntityCommandBuffer CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        for (int i = 0; i < workplaceEntities.Length; i++)
        {
            var workerData = workplaceWorkerDatas[i];

            if (workerData.CurrentWorkers < workerData.MaxWorkers && workerData.IsWorkable && math.all(workerData.WorkPosition != float3.zero))
            {
                int neededWorkers = workerData.MaxWorkers - workerData.CurrentWorkers;

                NativeArray<EntityDistanceInfo> closestCitizenIndexes = new NativeArray<EntityDistanceInfo>(neededWorkers, Allocator.TempJob);

                FindClosestCitizensJob job = new FindClosestCitizensJob
                {
                    NeededCitizens = neededWorkers,
                    CitizenTranslations = idleCitizensTranslations,
                    StartPosition = workerData.WorkPosition,
                    ClosestCitizenIndexes = closestCitizenIndexes
                };

                job.Schedule(citizenTranslationHandle).Complete();

                citizenEntititiesHandle.Complete();

                // Assign citizens to job
                foreach (var item in closestCitizenIndexes.Distinct())
                {
                    int citizenIndex = item.EntityIndex;

                    if (citizenIndex == -1)
                        continue;

                    CommandBuffer.AddComponent<CitizenWork>(idleCitizenEntitities[citizenIndex]);
                    CommandBuffer.SetComponent(idleCitizenEntitities[citizenIndex], new CitizenWork { WorkplaceEntity = workplaceEntities[i], WorkPosition = workerData.WorkPosition });

                    CommandBuffer.AddComponent<NavAgentRequestingPath>(idleCitizenEntitities[citizenIndex]);
                    CommandBuffer.SetComponent(idleCitizenEntitities[citizenIndex], new NavAgentRequestingPath { StartPosition = idleCitizensTranslations[citizenIndex].Value, EndPosition = workerData.WorkPosition });

                    CommandBuffer.RemoveComponent<IdleTag>(idleCitizenEntitities[citizenIndex]);
                    CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(idleCitizenEntitities[citizenIndex]);

                    // Save the worker count
                    workerData.CurrentWorkers++;
                    CommandBuffer.SetComponent(workplaceEntities[i], workerData);

                    // Prevents the same entity from being uses twice
                    idleCitizensTranslations[citizenIndex] = new Translation { };
                }

                closestCitizenIndexes.Dispose();
            }
        }

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();

        citizenEntititiesHandle.Complete();
        citizenTranslationHandle.Complete();

        idleCitizenEntitities.Dispose();
        idleCitizensTranslations.Dispose();

        workplaceEntities.Dispose();
        workplaceWorkerDatas.Dispose();
    }


    [BurstCompile]
    struct FindClosestCitizensJob : IJob
    {
        public int NeededCitizens;

        public float3 StartPosition;

        [ReadOnly]
        public NativeArray<Translation> CitizenTranslations;

        public NativeArray<EntityDistanceInfo> ClosestCitizenIndexes;

        public void Execute()
        {
            float distanceToBeat = Mathf.Infinity;
            int highestDistanceIndex = 0;

            for (int i = 0; i < ClosestCitizenIndexes.Length; i++)
            {
                ClosestCitizenIndexes[i] = new EntityDistanceInfo { EntityIndex = -1, Distance = Mathf.Infinity };
            }

            for (int i = 0; i < CitizenTranslations.Length; i++)
            {
                if (math.all(CitizenTranslations[i].Value == float3.zero))
                    continue;

                float distance = math.distance(StartPosition, CitizenTranslations[i].Value);

                if (distance < distanceToBeat)
                {
                    ClosestCitizenIndexes[highestDistanceIndex] = new EntityDistanceInfo { EntityIndex = i, Distance = distance };

                    // Set distanceToBeat to the highest distance
                    distanceToBeat = ClosestCitizenIndexes[0].Distance;
                    for (int d = 0; d < ClosestCitizenIndexes.Length; d++)
                    {
                        if (distanceToBeat < ClosestCitizenIndexes[d].Distance)
                        {
                            distanceToBeat = ClosestCitizenIndexes[d].Distance;
                            highestDistanceIndex = d;
                        }
                    }
                }
            }
        }
    }

    [Serializable]
    struct EntityDistanceInfo : IComparable<EntityDistanceInfo>
    {
        public int EntityIndex;
        public float Distance;

        public int CompareTo(EntityDistanceInfo other)
        {
            return Distance.CompareTo(other.Distance);
        }
    }
}
