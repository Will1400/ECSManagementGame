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

        Entities.WithAll<HasArrivedAtDestinationTag>().ForEach((Entity entity, int nativeThreadIndex, ref Translation translation, ref MovingToPickupResource movingToPickupResource, ref ResourceTransportJobData resourceTransportJob) =>
        {
            // Pickup
            CommandBuffer.RemoveComponent<MovingToPickupResource>(nativeThreadIndex, entity);
            CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(nativeThreadIndex, entity);

            CommandBuffer.AddComponent<ResourceBeingCarriedTag>(nativeThreadIndex, movingToPickupResource.ResourceEntity);
            CommandBuffer.AddComponent<IsCarryingResourceTag>(nativeThreadIndex, entity);

            CommandBuffer.AddComponent<CarrierData>(nativeThreadIndex, movingToPickupResource.ResourceEntity);
            CommandBuffer.SetComponent(nativeThreadIndex, movingToPickupResource.ResourceEntity, new CarrierData { Carrier = entity });

            // Move to destination
            CommandBuffer.AddComponent<NavAgentRequestingPath>(nativeThreadIndex, entity);
            CommandBuffer.SetComponent(nativeThreadIndex, entity, new NavAgentRequestingPath { StartPosition = translation.Value, EndPosition = resourceTransportJob.DestinationPosition });

        }).ScheduleParallel();
    }
}
