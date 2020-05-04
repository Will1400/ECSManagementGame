using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[UpdateAfter(typeof(FoodConsumptionSystem))]
public class FoodConsumptionCleanupSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob) ;

        Entities.ForEach((Entity entity, ref ConsumeFoodData consumeFoodData) =>
        {
            if (EntityManager.HasComponent<ResourceInStorageData>(consumeFoodData.FoodEntity))
            {
                var destroyFoodEntity = CommandBuffer.CreateEntity();
                CommandBuffer.AddComponent<DestroyResourceInStorageData>(destroyFoodEntity);
                CommandBuffer.SetComponent(destroyFoodEntity, new DestroyResourceInStorageData
                {
                    ResourceData = EntityManager.GetComponentData<ResourceData>(consumeFoodData.FoodEntity),
                    StorageEntity = EntityManager.GetComponentData<ResourceInStorageData>(consumeFoodData.FoodEntity).StorageEntity
                });
            }
            else
            {
                CommandBuffer.DestroyEntity(consumeFoodData.FoodEntity);
            }

            CommandBuffer.DestroyEntity(entity);

        }).WithoutBurst().Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }
}
