using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using System.Linq;

public class ResourceRequestTransportJobCreationSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery resourceRequestQuery;
    EntityQuery resourcesInStorageQuery;
    EntityQuery StorageAreasQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        resourceRequestQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceRequest) },
        });

        resourcesInStorageQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceData), typeof(ResourceInStorage), typeof(ResourceIsAvailableTag) },
        });

        StorageAreasQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceStorage), typeof(ResourceStorageAreaTag) },
        });
    }

    protected override void OnUpdate()
    {
        NativeArray<Entity> storageAreas = StorageAreasQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Entity> resourceEntities = resourcesInStorageQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<ResourceInStorage> resourcesInStorageAreas = resourcesInStorageQuery.ToComponentDataArray<ResourceInStorage>(Allocator.TempJob);

        var CommandBuffer = bufferSystem.CreateCommandBuffer();
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref ResourceRequest resourceRequest) =>
        {
            if (!EntityManager.Exists(resourceRequest.RequestingEntity))
            {
                CommandBuffer.DestroyEntity(entity);
                return;
            }

            var transportJob = new ResourceTransportJobData
            {
                DestinationEntity = resourceRequest.RequestingEntity,
                DestinationPosition = EntityManager.GetComponentData<Translation>(resourceRequest.RequestingEntity).Value,
            };

            for (int i = 0; i < resourcesInStorageAreas.Length; i++)
            {
                if (resourceRequest.Amount <= 0)
                    break;

                if (resourcesInStorageAreas[i].ResourceData.ResourceType == resourceRequest.ResourceType && resourceEntities[i] != Entity.Null)
                {
                    var resource = resourcesInStorageAreas[i];

                    if (resource.ResourceData.Amount <= resourceRequest.Amount)
                    {
                        transportJob.ResourceEntity = resourceEntities[i];

                        var jobEntity = CommandBuffer.CreateEntity();
                        CommandBuffer.AddComponent<ResourceTransportJobData>(jobEntity);
                        CommandBuffer.SetComponent(jobEntity, transportJob);

                        CommandBuffer.RemoveComponent<ResourceIsAvailableTag>(resourceEntities[i]);

                        resourceRequest.Amount -= resourcesInStorageAreas[i].ResourceData.Amount;

                        resourceEntities[i] = Entity.Null;
                        resourcesInStorageAreas[i] = new ResourceInStorage { StorageEntity = resource.StorageEntity };

                    }
                }
            }

            if (resourceRequest.Amount <= 0)
            {
                CommandBuffer.DestroyEntity(entity);
            }

        }).WithoutBurst().Run();

        storageAreas.Dispose();
        resourcesInStorageAreas.Dispose();
        resourceEntities.Dispose();
    }
}
