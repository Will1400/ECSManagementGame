using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public class EntityArrivedAtFoodSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        Entities.WithAll<HasArrivedAtDestinationTag>().ForEach((Entity entity, ref MovingToEatFoodData movingToEatFoodData) =>
        {
            var consumptionEntity = CommandBuffer.CreateEntity();
            CommandBuffer.AddComponent<ConsumeFoodData>(consumptionEntity);
            CommandBuffer.SetComponent(consumptionEntity, new ConsumeFoodData
            {
                ConsumerEntity = entity,
                FoodEntity = movingToEatFoodData.FoodEntity,
                FoodData = EntityManager.GetComponentData<FoodData>(movingToEatFoodData.FoodEntity)
            });

            CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(entity);
        }).WithoutBurst().Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }
}
