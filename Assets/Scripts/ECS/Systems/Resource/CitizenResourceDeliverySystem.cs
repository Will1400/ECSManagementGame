using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;

[UpdateAfter(typeof(CitizenResourcePickupSystem))]
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
                var resourceBuffer = EntityManager.GetBuffer<ResourceEntityIndexElement>(transportJobData.DestinationEntity);
                var resourceIndex = resourceBuffer.Length;
                resourceBuffer.Add(transportJobData.ResourceEntity.Index);

                var storageAreaStart = EntityManager.GetComponentData<GridOccupation>(transportJobData.DestinationEntity).Start;

                CommandBuffer.AddComponent<ResourceInStorage>(transportJobData.ResourceEntity);
                CommandBuffer.SetComponent(transportJobData.ResourceEntity, new ResourceInStorage
                {
                    StorageEntityIndex = resourceIndex,
                    StorageEntity = transportJobData.DestinationEntity,
                    ResourceData = EntityManager.GetComponentData<ResourceData>(transportJobData.ResourceEntity),
                    StorageAreaStartPosition = new float3(storageAreaStart.x, 0.5f, storageAreaStart.y)
                });
            }

            CommandBuffer.RemoveComponent<ResourceTransportJobData>(citizen);
            CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(citizen);
            CommandBuffer.RemoveComponent<IsCarryingResourceTag>(citizen);

            CommandBuffer.RemoveComponent<CarrierData>(transportJobData.ResourceEntity);
            CommandBuffer.RemoveComponent<ResourceBeingCarriedTag>(transportJobData.ResourceEntity);

            CommandBuffer.AddComponent<IdleTag>(citizen);

        }).WithoutBurst().Run();
    }
}
