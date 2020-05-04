using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public class FoodConsumtionSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;
    EntityQuery consumptionDataQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        consumptionDataQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ConsumeFoodData) }
        });
    }

    protected override void OnUpdate()
    {
        if (consumptionDataQuery.CalculateChunkCount() == 0)
            return;

        var CommandBuffer = bufferSystem.CreateCommandBuffer();

        var consumptionEntities = consumptionDataQuery.ToEntityArray(Allocator.TempJob);
        var consumptionData = consumptionDataQuery.ToComponentDataArray<ConsumeFoodData>(Allocator.TempJob);

        Entities.ForEach((Entity entity, ref CitizenFoodData citizenFoodData, ref MovingToEatFoodData movingToEatFoodData) =>
        {
            for (int i = 0; i < consumptionData.Length; i++)
            {
                if (consumptionData[i].FoodEntity == movingToEatFoodData.FoodEntity)
                {
                    citizenFoodData.CurrentFoodLevel += consumptionData[i].FoodData.HungerReplenished;
                    CommandBuffer.DestroyEntity(consumptionEntities[i]);

                    CommandBuffer.RemoveComponent<MovingToEatFoodData>(entity);
                    CommandBuffer.AddComponent<IdleTag>(entity);
                }
            }
        }).Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.ShouldPlayback = false;

        consumptionEntities.Dispose();
        consumptionData.Dispose();
    }
}
