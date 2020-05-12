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

        var idleCitizens = idleCitizensQuery.ToEntityArrayAsync(Allocator.TempJob, out JobHandle citizenEntititiesHandle);
        var idleCitizensTranslation = idleCitizensQuery.ToComponentDataArrayAsync<Translation>(Allocator.TempJob, out JobHandle citizenTranslationHandle);

        var workplaceEntities = needsWorkersQuery.ToEntityArray(Allocator.TempJob);
        var workplaceWorkerDatas = needsWorkersQuery.ToComponentDataArray<WorkplaceWorkerData>(Allocator.TempJob);

        EntityCommandBuffer CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        for (int i = 0; i < workplaceEntities.Length; i++)
        {
            var workerData = workplaceWorkerDatas[i];

            if (workerData.CurrentWorkers < workerData.MaxWorkers && workerData.IsWorkable && math.all(workerData.WorkPosition != float3.zero))
            {
                int neededWorkers = workerData.MaxWorkers - workerData.CurrentWorkers;

                NativeArray<EntityDistanceInfo> nearestCitizenIndexes = new NativeArray<EntityDistanceInfo>(neededWorkers, Allocator.TempJob);

                FindNearestCitizensJob nearestCitizensJob = new FindNearestCitizensJob
                {
                    NeededCitizens = neededWorkers,
                    CitizenTranslations = idleCitizensTranslation,
                    StartPosition = workerData.WorkPosition,
                    ClosestCitizenIndexes = nearestCitizenIndexes
                };

                nearestCitizensJob.Schedule(citizenTranslationHandle).Complete();

                citizenEntititiesHandle.Complete();

                foreach (var item in nearestCitizenIndexes.Distinct())
                {
                    int citizenIndex = item.EntityIndex;

                    if (citizenIndex == -1)
                        continue;

                    CommandBuffer.AddComponent<CitizenWork>(idleCitizens[citizenIndex]);
                    CommandBuffer.SetComponent(idleCitizens[citizenIndex], new CitizenWork { WorkplaceEntity = workplaceEntities[i], WorkPosition = workerData.WorkPosition });

                    CommandBuffer.AddComponent<NavAgentRequestingPath>(idleCitizens[citizenIndex]);
                    CommandBuffer.SetComponent(idleCitizens[citizenIndex], new NavAgentRequestingPath { StartPosition = idleCitizensTranslation[citizenIndex].Value, EndPosition = workerData.WorkPosition });

                    CommandBuffer.RemoveComponent<IdleTag>(idleCitizens[citizenIndex]);
                    CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(idleCitizens[citizenIndex]);

                    // Save the worker count
                    workerData.CurrentWorkers++;
                    CommandBuffer.SetComponent(workplaceEntities[i], workerData);

                    // Prevents the same entity from being uses twice
                    idleCitizensTranslation[citizenIndex] = new Translation { };
                }

                nearestCitizenIndexes.Dispose();
            }
        }

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();

        citizenEntititiesHandle.Complete();
        citizenTranslationHandle.Complete();

        idleCitizens.Dispose();
        idleCitizensTranslation.Dispose();

        workplaceEntities.Dispose();
        workplaceWorkerDatas.Dispose();
    }


    [BurstCompile]
    struct FindNearestCitizensJob : IJob
    {
        public int NeededCitizens;

        public float3 StartPosition;

        public NativeArray<Translation> CitizenTranslations;

        public NativeArray<EntityDistanceInfo> ClosestCitizenIndexes;

        public void Execute()
        {
            float distanceToBeat = Mathf.Infinity;

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
                    for (int j = 0; j < ClosestCitizenIndexes.Length; j++)
                    {
                        if (distance < ClosestCitizenIndexes[j].Distance)
                        {
                            ClosestCitizenIndexes[j] = new EntityDistanceInfo { EntityIndex = i, Distance = distance };
                            break;
                        }
                    }
                    distanceToBeat = ClosestCitizenIndexes[0].Distance;

                    for (int d = 0; d < ClosestCitizenIndexes.Length; d++)
                    {
                        if (distanceToBeat < ClosestCitizenIndexes[d].Distance)
                        {
                            distanceToBeat = ClosestCitizenIndexes[d].Distance;
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
