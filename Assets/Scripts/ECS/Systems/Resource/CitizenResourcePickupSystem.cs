using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

public class CitizenResourcePickupSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery resourcesToMoveIntoStorageQuery;
    EntityQuery citizensReadyToPickupQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        //resourcesToMoveIntoStorageQuery = GetEntityQuery(new EntityQueryDesc
        //{
        //    All = new ComponentType[] { typeof(ResourceData), typeof(TransportResourceToStorageTag), typeof(Translation) },
        //});

        citizensReadyToPickupQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen), typeof(MovingToPickupResource), typeof(HasArrivedAtDestinationTag) }
        });
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.Concurrent CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent();

        Entities.WithAll<HasArrivedAtDestinationTag>().ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref MovingToPickupResource movingToPickupResource, ref ResourceTransportJobData resourceTransportJob) =>
        {
            // Pickup
            CommandBuffer.RemoveComponent<MovingToPickupResource>(entityInQueryIndex, entity);
            CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(entityInQueryIndex, entity);

            CommandBuffer.RemoveComponent<TransportResourceToStorageTag>(entityInQueryIndex, movingToPickupResource.ResourceEntity);

            CommandBuffer.AddComponent<ResourceBeingCarriedTag>(entityInQueryIndex, movingToPickupResource.ResourceEntity);
            CommandBuffer.AddComponent<IsCarryingResourceTag>(entityInQueryIndex, entity);

            CommandBuffer.AddComponent<CarrierData>(entityInQueryIndex, movingToPickupResource.ResourceEntity);
            CommandBuffer.SetComponent(entityInQueryIndex, movingToPickupResource.ResourceEntity, new CarrierData { Carrier = entity });

            // Move to destination
            CommandBuffer.AddComponent<NavAgentRequestingPath>(entityInQueryIndex, entity);
            CommandBuffer.SetComponent(entityInQueryIndex, entity, new NavAgentRequestingPath { StartPosition = translation.Value, EndPosition = resourceTransportJob.DestinationPosition });

        }).Schedule();
    }
}
