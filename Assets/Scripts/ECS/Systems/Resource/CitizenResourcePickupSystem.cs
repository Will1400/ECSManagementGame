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
        EntityCommandBuffer.Concurrent CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent();

        Entities.WithAll<HasArrivedAtDestinationTag, MovingToPickupResource>().ForEach((Entity entity, int entityInQueryIndex, ref Translation translation, ref ResourceTransportJobData resourceTransportJob) =>
        {
            var removeEntity = CommandBuffer.CreateEntity(entityInQueryIndex);
            CommandBuffer.AddComponent<RemoveResourceFromStorage>(entityInQueryIndex, removeEntity);
            CommandBuffer.SetComponent(entityInQueryIndex, removeEntity, new RemoveResourceFromStorage
            {
                ResourceEntity = resourceTransportJob.ResourceEntity,
            });

            // Pickup
            CommandBuffer.RemoveComponent<MovingToPickupResource>(entityInQueryIndex, entity);
            CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(entityInQueryIndex, entity);

            CommandBuffer.RemoveComponent<TransportResourceToStorageTag>(entityInQueryIndex, resourceTransportJob.ResourceEntity);

            CommandBuffer.AddComponent<ResourceBeingCarriedTag>(entityInQueryIndex, resourceTransportJob.ResourceEntity);
            CommandBuffer.AddComponent<IsCarryingResourceTag>(entityInQueryIndex, entity);

            CommandBuffer.AddComponent<CarrierData>(entityInQueryIndex, resourceTransportJob.ResourceEntity);
            CommandBuffer.SetComponent(entityInQueryIndex, resourceTransportJob.ResourceEntity, new CarrierData { Carrier = entity });

            // Move to destination
            CommandBuffer.AddComponent<NavAgentRequestingPath>(entityInQueryIndex, entity);
            CommandBuffer.SetComponent(entityInQueryIndex, entity, new NavAgentRequestingPath { StartPosition = translation.Value, EndPosition = resourceTransportJob.DestinationPosition });

        }).Schedule();
    }
}
