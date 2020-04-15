using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;

public class ResourceTransportJobAssignmentSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery transportJobsQuery;
    EntityQuery idleCitizensQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        transportJobsQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceTransportJobData) },
            None = new ComponentType[] { typeof(Citizen) }
        });


        idleCitizensQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen), typeof(IdleTag) },
            None = new ComponentType[] { typeof(MovingToPickupResource) }
        });
    }

    protected override void OnUpdate()
    {
        NativeArray<Entity> idleCitizens = idleCitizensQuery.ToEntityArray(Allocator.Persistent);
        EntityCommandBuffer CommandBuffer = bufferSystem.CreateCommandBuffer();

        int citizenIndex = 0;
        Entities.WithNone<Citizen>().ForEach((Entity entity, ref ResourceTransportJobData transportJobData) =>
        {
            if (citizenIndex >= idleCitizens.Length)
                return;

            var citizen = idleCitizens[citizenIndex];

            if (math.all(transportJobData.DestinationPosition == float3.zero))
            {
                transportJobData.DestinationPosition = EntityManager.GetComponentData<Translation>(transportJobData.DestinationEntity).Value;
            }

            if (EntityManager.HasComponent<ResourceStorageData>(transportJobData.DestinationEntity))
            {
                var storageData = EntityManager.GetComponentData<ResourceStorageData>(transportJobData.DestinationEntity);

                if (storageData.UsedCapacity >= storageData.MaxCapacity && storageData.MaxCapacity != -1)
                {
                    CommandBuffer.AddComponent<ResourceStorageFullTag>(transportJobData.DestinationEntity);
                    return;
                }
                else
                {
                    storageData.UsedCapacity++;
                    CommandBuffer.SetComponent(transportJobData.DestinationEntity, storageData);
                }
            }

            NavAgentRequestingPath requestingPath = new NavAgentRequestingPath
            {
                StartPosition = EntityManager.GetComponentData<Translation>(citizen).Value,
                EndPosition = EntityManager.GetComponentData<Translation>(transportJobData.ResourceEntity).Value
            };

            CommandBuffer.AddComponent<MovingToPickupResource>(citizen);
            CommandBuffer.SetComponent(citizen, new MovingToPickupResource { ResourceEntity = transportJobData.ResourceEntity });

            CommandBuffer.AddComponent<NavAgentRequestingPath>(citizen);
            CommandBuffer.SetComponent(citizen, requestingPath);

            CommandBuffer.AddComponent<ResourceTransportJobData>(citizen);
            CommandBuffer.SetComponent(citizen, transportJobData);

            CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(citizen);
            CommandBuffer.RemoveComponent<IdleTag>(citizen);

            CommandBuffer.DestroyEntity(entity);

            citizenIndex++;
        }).WithoutBurst().Run();

        // Needs to be played back here to remove IdleTag immediately
        CommandBuffer.Playback(EntityManager);
        CommandBuffer.ShouldPlayback = false;

        idleCitizens.Dispose();
    }
}
