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
        Entities.ForEach((Entity entity, ref RemoveResourceFromStorage removeResourceFromStorage) =>
        {
            var storageEntity = EntityManager.GetComponentData<ResourceInStorage>(removeResourceFromStorage.ResourceEntity).StorageEntity;
            var resourceStorage = EntityManager.GetComponentData<ResourceStorage>(storageEntity);
            resourceStorage.UsedCapacity--;

            EntityManager.SetComponentData(storageEntity, resourceStorage);

            CommandBuffer.DestroyEntity(entity);
        }).WithoutBurst().Run();
    }
}
