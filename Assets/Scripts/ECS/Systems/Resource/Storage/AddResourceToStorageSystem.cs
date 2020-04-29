using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class AddResourceToStorageSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer CommandBuffer = bufferSystem.CreateCommandBuffer();

        Entities.ForEach((Entity entity, ref AddResourceToStorageData addResourceToStorage) =>
        {
            ResourceData resourceData = EntityManager.GetComponentData<ResourceData>(addResourceToStorage.ResourceEntity);
            var position = EntityManager.GetComponentData<Translation>(addResourceToStorage.StorageEntity).Value;

            // Set storage capacity
            var resourceStorage = EntityManager.GetComponentData<ResourceStorageData>(addResourceToStorage.StorageEntity);
            resourceStorage.UsedCapacity++;
            CommandBuffer.SetComponent(addResourceToStorage.StorageEntity, resourceStorage);

            // Add to storage
            var resourceBuffer = EntityManager.GetBuffer<ResourceDataElement>(addResourceToStorage.StorageEntity);
            Debug.Log("Buffer length before adding: " + resourceBuffer.Length);
            resourceBuffer.Add(new ResourceDataElement { Value = resourceData });
            Debug.Log("Buffer length after adding: " + resourceBuffer.Length);

            CommandBuffer.AddComponent<ResourceInStorageData>(addResourceToStorage.ResourceEntity);
            CommandBuffer.SetComponent(addResourceToStorage.ResourceEntity, new ResourceInStorageData
            {
                StorageEntity = addResourceToStorage.StorageEntity,
                ResourceData = resourceData,
                StorageAreaStartPosition = position,
                StorageAreaEndPosition = position + new float3(1, 1, 1),
            });

            CommandBuffer.DestroyEntity(entity);
        }).WithoutBurst().Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.ShouldPlayback = false;
    }
}
