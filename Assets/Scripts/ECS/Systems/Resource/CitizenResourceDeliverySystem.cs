using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Transforms;

[UpdateAfter(typeof(CitizenResourcePickupSystem))]
[UpdateInGroup(typeof(ResourceInteractionGroup))]
public class CitizenResourceDeliverySystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery citizensReadyForDeliveryQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        citizensReadyForDeliveryQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen), typeof(HasArrivedAtDestinationTag), typeof(ResourceTransportJobData) }
        });
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = bufferSystem.CreateCommandBuffer();
        Entities.WithAll<Citizen, HasArrivedAtDestinationTag>().WithNone<MovingToPickupResource>().ForEach((Entity citizen, ref ResourceTransportJobData transportJobData) =>
        {
            if (EntityManager.Exists(transportJobData.DestinationEntity))
            {
                if (!EntityManager.HasComponent<ResourceStorageAreaTag>(transportJobData.DestinationEntity))
                {
                    var resourceBuffer = EntityManager.GetBuffer<ResourceDataElement>(transportJobData.DestinationEntity);
                    resourceBuffer.Add(EntityManager.GetComponentData<ResourceData>(transportJobData.ResourceEntity));
                }
                else
                {
                    CommandBuffer.AddComponent<ResourceIsAvailableTag>(transportJobData.ResourceEntity);
                }

                var occupation = EntityManager.GetComponentData<GridOccupation>(transportJobData.DestinationEntity);

                CommandBuffer.AddComponent<ResourceInStorageData>(transportJobData.ResourceEntity);
                CommandBuffer.SetComponent(transportJobData.ResourceEntity, new ResourceInStorageData
                {
                    StorageEntity = transportJobData.DestinationEntity,
                    ResourceData = EntityManager.GetComponentData<ResourceData>(transportJobData.ResourceEntity),
                    StorageAreaStartPosition = new float3(occupation.Start.x, 0.5f, occupation.Start.y),
                    StorageAreaEndPosition = new float3(occupation.End.x, 0, occupation.End.y)
                });
            }

            CommandBuffer.SetComponent(transportJobData.ResourceEntity, new Rotation { Value = quaternion.identity });

            
            CommandBuffer.RemoveComponent<ResourceTransportJobData>(citizen);
            CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(citizen);
            CommandBuffer.RemoveComponent<IsCarryingResourceTag>(citizen);

            CommandBuffer.RemoveComponent<ResourceIsUnderTransportationTag>(transportJobData.ResourceEntity);

            CommandBuffer.AddComponent<IdleTag>(citizen);

        }).WithoutBurst().Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.ShouldPlayback = false;
    }
}
