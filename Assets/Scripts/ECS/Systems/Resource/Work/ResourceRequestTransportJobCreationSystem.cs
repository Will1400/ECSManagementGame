using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using System.Linq;

[UpdateInGroup(typeof(WorkCreationGroup))]
public class ResourceRequestTransportJobCreationSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery resourcesInStorageQuery;
    EntityQuery resourceRequestsQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        resourcesInStorageQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceData), typeof(ResourceInStorageData), typeof(ResourceIsAvailableTag) },
        });

        resourceRequestsQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceRequestData) },
        });
    }

    protected override void OnUpdate()
    {
        if (resourcesInStorageQuery.CalculateChunkCount() == 0 || resourceRequestsQuery.CalculateEntityCount() == 0)
            return;

        NativeArray<Entity> resourceEntities = resourcesInStorageQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<ResourceInStorageData> resourcesInStorageAreas = resourcesInStorageQuery.ToComponentDataArray<ResourceInStorageData>(Allocator.TempJob);

        var CommandBuffer = bufferSystem.CreateCommandBuffer();

        // Destroy request if the entity does not exist
        Entities.ForEach((Entity entity, ref ResourceRequestData resourceRequest) =>
        {
            if (!EntityManager.Exists(resourceRequest.RequestingEntity))
            {
                CommandBuffer.DestroyEntity(entity);
            }

        }).WithoutBurst().Run();

        // Try to fulfill requests
        Entities.ForEach((Entity entity, ref ResourceRequestData resourceRequest) =>
        {
            var transportJob = new ResourceTransportJobData
            {
                DestinationEntity = resourceRequest.RequestingEntity,
                DestinationPosition = resourceRequest.RequestingEntityPosition,
            };

            for (int i = 0; i < resourcesInStorageAreas.Length; i++)
            {
                if (resourceRequest.Amount <= 0)
                    break;

                // Check if resource matches the requested type
                if (resourcesInStorageAreas[i].ResourceData.ResourceType == resourceRequest.ResourceType && resourceEntities[i] != Entity.Null)
                {
                    var resource = resourcesInStorageAreas[i];

                    if (resource.ResourceData.Amount <= resourceRequest.Amount)
                    {
                        // Found resource to use for request
                        transportJob.ResourceEntity = resourceEntities[i];

                        var jobEntity = CommandBuffer.CreateEntity();
                        CommandBuffer.AddComponent<ResourceTransportJobData>(jobEntity);
                        CommandBuffer.SetComponent(jobEntity, transportJob);

                        CommandBuffer.RemoveComponent<ResourceIsAvailableTag>(resourceEntities[i]);

                        resourceRequest.Amount -= resourcesInStorageAreas[i].ResourceData.Amount;

                        resourceEntities[i] = Entity.Null;
                        resourcesInStorageAreas[i] = new ResourceInStorageData { StorageEntity = resource.StorageEntity };
                    }
                }
            }

            if (resourceRequest.Amount <= 0)
            {
                // Request fulfilled
                CommandBuffer.DestroyEntity(entity);
            }

        }).Run();

        resourcesInStorageAreas.Dispose();
        resourceEntities.Dispose();
    }
}
