using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine.Rendering;

[UpdateInGroup(typeof(WorkAssignmentGroup))]
public class ResourceTransportJobAssignmentSystem : SystemBase
{
    EntityQuery transportJobsQuery;
    EntityQuery idleCitizensQuery;

    protected override void OnCreate()
    {
        transportJobsQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceTransportJobData) },
            None = new ComponentType[] { typeof(Citizen) }
        });

        idleCitizensQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen), typeof(IdleTag), typeof(Translation) },
            None = new ComponentType[] { typeof(MovingToPickupResource) }
        });
    }

    protected override void OnUpdate()
    {
        if (idleCitizensQuery.CalculateEntityCount() == 0 || transportJobsQuery.CalculateEntityCount() == 0)
            return;

        NativeArray<Entity> idleCitizens = idleCitizensQuery.ToEntityArray(Allocator.TempJob);
        var idleCitizenTranslations = idleCitizensQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        var transportJobEntities = transportJobsQuery.ToEntityArray(Allocator.TempJob);
        var transportJobDatas = transportJobsQuery.ToComponentDataArray<ResourceTransportJobData>(Allocator.TempJob);

        EntityCommandBuffer CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);


        NativeArray<int> closestPositionArray = new NativeArray<int>(1, Allocator.TempJob);

        for (int i = 0; i < transportJobDatas.Length; i++)
        {
            if (math.all(transportJobDatas[i].ResourcePosition == float3.zero))
                continue;
            
            closestPositionArray[0] = -1;
            var job = new FindClosestPositionJob
            {
                CitizenTranslations = idleCitizenTranslations,
                StartPosition = transportJobDatas[i].ResourcePosition,
                ClosestIndexArray = closestPositionArray
            };

            job.Run();

            if (closestPositionArray[0] == -1)
                continue;

            var citizen = idleCitizens[closestPositionArray[0]];
            if (EntityManager.HasComponent<ResourceStorageData>(transportJobDatas[i].DestinationEntity))
            {
                var storageData = EntityManager.GetComponentData<ResourceStorageData>(transportJobDatas[i].DestinationEntity);

                storageData.UsedCapacity++;
                CommandBuffer.SetComponent(transportJobDatas[i].DestinationEntity, storageData);
            }

            NavAgentRequestingPath requestingPath = new NavAgentRequestingPath
            {
                StartPosition = EntityManager.GetComponentData<Translation>(citizen).Value,
                EndPosition = transportJobDatas[i].ResourcePosition
            };

            CommandBuffer.AddComponent<MovingToPickupResource>(citizen);
            CommandBuffer.SetComponent(citizen, new MovingToPickupResource { ResourceEntity = transportJobDatas[i].ResourceEntity });

            CommandBuffer.AddComponent<NavAgentRequestingPath>(citizen);
            CommandBuffer.SetComponent(citizen, requestingPath);

            CommandBuffer.AddComponent<ResourceTransportJobData>(citizen);
            CommandBuffer.SetComponent(citizen, transportJobDatas[i]);

            CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(citizen);
            CommandBuffer.RemoveComponent<IdleTag>(citizen);

            CommandBuffer.DestroyEntity(transportJobEntities[i]);

            // Prevents the same entity from being uses twice
            idleCitizenTranslations[closestPositionArray[0]] = new Translation { };
        }

        closestPositionArray.Dispose();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();

        idleCitizens.Dispose();
        idleCitizenTranslations.Dispose();

        transportJobEntities.Dispose();
        transportJobDatas.Dispose();
    }

    [BurstCompile]
    struct FindClosestPositionJob : IJob
    {
        public float3 StartPosition;

        public NativeArray<Translation> CitizenTranslations;

        public NativeArray<int> ClosestIndexArray;

        public void Execute()
        {
            float distanceToBeat = Mathf.Infinity;

            for (int i = 0; i < CitizenTranslations.Length; i++)
            {
                if (math.all(CitizenTranslations[i].Value == float3.zero))
                    continue;

                float distance = math.distance(StartPosition, CitizenTranslations[i].Value);

                if (distance < distanceToBeat)
                {
                    ClosestIndexArray[0] = i; ;
                    distanceToBeat = distance;
                }
            }
        }
    }
}
