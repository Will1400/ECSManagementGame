using UnityEngine;
using System.Collections;
using System.Linq;
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
        EntityCommandBuffer CommandBuffer = bufferSystem.CreateCommandBuffer();

        Entities.WithAll<HasArrivedAtDestinationTag, MovingToPickupResource>().ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref ResourceTransportJobData resourceTransportJob) =>
        {
            var removeEntity = CommandBuffer.CreateEntity();
            CommandBuffer.AddComponent<RemoveResourceFromStorage>(removeEntity);
            CommandBuffer.SetComponent(removeEntity, new RemoveResourceFromStorage { ResourceEntity = resourceTransportJob.ResourceEntity });

            // Pickup
            CommandBuffer.RemoveComponent<MovingToPickupResource>(entity);
            CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(entity);

            CommandBuffer.RemoveComponent<TransportResourceToStorageTag>(resourceTransportJob.ResourceEntity);

            CommandBuffer.AddComponent<ResourceBeingCarriedTag>(resourceTransportJob.ResourceEntity);
            CommandBuffer.AddComponent<IsCarryingResourceTag>(entity);

            CommandBuffer.AddComponent<CarrierData>(resourceTransportJob.ResourceEntity);
            CommandBuffer.SetComponent(resourceTransportJob.ResourceEntity, new CarrierData { Carrier = entity });

            // Move to destination
            CommandBuffer.AddComponent<NavAgentRequestingPath>(entity);
            CommandBuffer.SetComponent(entity, new NavAgentRequestingPath { StartPosition = translation.Value, EndPosition = resourceTransportJob.DestinationPosition });

        }).WithoutBurst().Run();

        //Entities.WithAll<HasArrivedAtDestinationTag>().ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref MovingToPickupResource movingToPickupResource, ref ResourceTransportJobData resourceTransportJob) =>
        //{
        //    // Pickup
        //    CommandBuffer.RemoveComponent<MovingToPickupResource>(entity);
        //    CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(entity);

        //    CommandBuffer.RemoveComponent<TransportResourceToStorageTag>(movingToPickupResource.ResourceEntity);

        //    CommandBuffer.AddComponent<ResourceBeingCarriedTag>(movingToPickupResource.ResourceEntity);
        //    CommandBuffer.AddComponent<IsCarryingResourceTag>(entity);

        //    CommandBuffer.AddComponent<CarrierData>(movingToPickupResource.ResourceEntity);
        //    CommandBuffer.SetComponent(movingToPickupResource.ResourceEntity, new CarrierData { Carrier = entity });

        //    // Move to destination
        //    CommandBuffer.AddComponent<NavAgentRequestingPath>(entity);
        //    CommandBuffer.SetComponent(entity, new NavAgentRequestingPath { StartPosition = translation.Value, EndPosition = resourceTransportJob.DestinationPosition });

        //}).Run();
    }
}
