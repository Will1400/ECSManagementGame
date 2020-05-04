using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;

public class EntityArrivedAtFoodSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;
    EntityQuery entitiesArrivedAtFoodQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        entitiesArrivedAtFoodQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(MovingToEatFoodData), typeof(CitizenFoodData), typeof(Translation), typeof(HasArrivedAtDestinationTag) },
        });
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = bufferSystem.CreateCommandBuffer();

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
        CommandBuffer.ShouldPlayback = false;
    }
}
