using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

public class FindFoodForHungryEntitiesSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;
    EntityQuery foodQuery;
    EntityQuery hungryEntitiesQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        foodQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(FoodData), typeof(Translation) }
        });

        hungryEntitiesQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(IsHungryTag), typeof(CitizenFoodData), typeof(Translation) },
            None = new ComponentType[] { typeof(MovingToEatFoodData) }
        });
    }

    protected override void OnUpdate()
    {
        if (foodQuery.CalculateChunkCount() == 0 || hungryEntitiesQuery.CalculateChunkCount() == 0)
            return;

        var foodEntities = foodQuery.ToEntityArray(Allocator.TempJob);
        var foodDatas = foodQuery.ToComponentDataArray<FoodData>(Allocator.TempJob);
        var foodTranslations = foodQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        var hungryEntities = hungryEntitiesQuery.ToEntityArray(Allocator.TempJob);
        var hungryEntitiesTranslations = hungryEntitiesQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        for (int i = 0; i < hungryEntities.Length; i++)
        {
            FindNearestFoodJob job = new FindNearestFoodJob
            {
                FoodTranslations = foodTranslations,
                HungryEntityPosition = hungryEntitiesTranslations[i].Value
            };
            job.Schedule().Complete();
            //job.Run();

            if (job.ClosestFoodIndex == -1)
            {
                continue;
            }

            CommandBuffer.RemoveComponent<IsHungryTag>(hungryEntities[i]);

            CommandBuffer.AddComponent<MovingToEatFoodData>(hungryEntities[i]);
            CommandBuffer.SetComponent(hungryEntities[i], new MovingToEatFoodData { FoodEntity = foodEntities[job.ClosestFoodIndex] });

            CommandBuffer.AddComponent<NavAgentRequestingPath>(hungryEntities[i]);
            CommandBuffer.SetComponent(hungryEntities[i], new NavAgentRequestingPath
            {
                StartPosition = job.HungryEntityPosition,
                EndPosition = foodTranslations[job.ClosestFoodIndex].Value
            });
        }

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();

        foodEntities.Dispose();
        foodDatas.Dispose();
        foodTranslations.Dispose();

        hungryEntities.Dispose();
        hungryEntitiesTranslations.Dispose();
    }

    [BurstCompile]
    struct FindNearestFoodJob : IJob
    {
        public NativeArray<Translation> FoodTranslations;

        public float3 HungryEntityPosition;

        public int ClosestFoodIndex;

        public void Execute()
        {
            float shortestDistance = Mathf.Infinity;

            for (int i = 0; i < FoodTranslations.Length; i++)
            {
                float distance = math.distance(HungryEntityPosition, FoodTranslations[i].Value);

                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    ClosestFoodIndex = i;
                }
            }
        }
    }
}
