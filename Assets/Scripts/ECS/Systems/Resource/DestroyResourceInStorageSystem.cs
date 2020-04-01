using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

public class DestroyResourceInStorageSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery resourcesInStorageQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        resourcesInStorageQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceData), typeof(ResourceInStorage) }
        });
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent();
        var resourcesInStorage = resourcesInStorageQuery.ToComponentDataArray<ResourceInStorage>(Allocator.TempJob);
        var resourcesInStorageEntities = resourcesInStorageQuery.ToEntityArray(Allocator.TempJob);

        Entities.ForEach((Entity entity, int entityInQueryIndex, ref DestroyResourceInStorage resourceToDestroyData) =>
        {
            for (int i = 0; i < resourcesInStorage.Length; i++)
            {
                var resource = resourcesInStorage[i];
                if (resourcesInStorage[i].StorageEntity.Index == resourceToDestroyData.StorageId)
                {
                    CommandBuffer.DestroyEntity(entityInQueryIndex, resourcesInStorageEntities[i]);
                }
            }
            CommandBuffer.DestroyEntity(entityInQueryIndex, entity);

        }).Schedule(Dependency).Complete();

        resourcesInStorage.Dispose();
        resourcesInStorageEntities.Dispose();
    }
}
