using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

[UpdateAfter(typeof(EntityArrivedAtFoodSystem))]
public class FoodConsumptionSystem : SystemBase
{
    EntityQuery consumptionDataQuery;

    protected override void OnCreate()
    {

        consumptionDataQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ConsumeFoodData) }
        });
    }

    protected override void OnUpdate()
    {
        if (consumptionDataQuery.CalculateChunkCount() == 0)
            return;

        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        var consumptionData = consumptionDataQuery.ToComponentDataArray<ConsumeFoodData>(Allocator.TempJob);

        Entities.ForEach((Entity entity, ref CitizenFoodData citizenFoodData, ref MovingToEatFoodData movingToEatFoodData) =>
        {
            for (int i = 0; i < consumptionData.Length; i++)
            {
                if (consumptionData[i].FoodEntity == movingToEatFoodData.FoodEntity)
                {
                    citizenFoodData.CurrentFoodLevel += consumptionData[i].FoodData.HungerReplenished;

                    // Reset citizen
                    CommandBuffer.RemoveComponent<MovingToEatFoodData>(entity);
                    CommandBuffer.AddComponent<IdleTag>(entity);
                }
            }
        }).Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();

        consumptionData.Dispose();
    }
}
