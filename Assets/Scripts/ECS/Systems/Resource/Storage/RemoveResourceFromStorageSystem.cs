using UnityEngine;
using System.Collections;
using Unity.Entities;

public class RemoveResourceFromStorageSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer CommandBuffer = bufferSystem.CreateCommandBuffer();
        Entities.ForEach((Entity entity, ref RemoveResourceFromStorageData removeResourceFromStorage) =>
        {
            var storageEntity = EntityManager.GetComponentData<ResourceInStorageData>(removeResourceFromStorage.ResourceEntity).StorageEntity;
            var resourceStorage = EntityManager.GetComponentData<ResourceStorageData>(storageEntity);
            resourceStorage.UsedCapacity--;

            CommandBuffer.RemoveComponent<ResourceInStorageData>(removeResourceFromStorage.ResourceEntity);

            CommandBuffer.SetComponent(storageEntity, resourceStorage);

            CommandBuffer.DestroyEntity(entity);
        }).WithoutBurst().Run();
    }
}
