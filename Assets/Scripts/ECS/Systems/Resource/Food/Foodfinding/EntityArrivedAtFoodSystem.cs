﻿using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public class EntityArrivedAtFoodSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        Entities.WithAll<HasArrivedAtDestinationTag>().ForEach((Entity entity, ref MovingToEatFoodData movingToEatFoodData) =>
        {
            if (EntityManager.Exists(movingToEatFoodData.FoodEntity))
            {
                var consumptionEntity = CommandBuffer.CreateEntity();
                CommandBuffer.AddComponent<ConsumeFoodData>(consumptionEntity);
                CommandBuffer.SetComponent(consumptionEntity, new ConsumeFoodData
                {
                    ConsumerEntity = entity,
                    FoodEntity = movingToEatFoodData.FoodEntity,
                    FoodData = EntityManager.GetComponentData<FoodData>(movingToEatFoodData.FoodEntity)
                });
            }
            else
            {
                CommandBuffer.RemoveComponent<MovingToEatFoodData>(entity);
                CommandBuffer.AddComponent<IdleTag>(entity);
            }

            CommandBuffer.RemoveComponent<HasArrivedAtDestinationTag>(entity);
        }).WithoutBurst().Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }
}
