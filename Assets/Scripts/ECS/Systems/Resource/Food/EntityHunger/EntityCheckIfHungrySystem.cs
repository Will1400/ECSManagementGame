using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

[UpdateBefore(typeof(WorkAssignmentGroup))]
public class EntityCheckIfHungrySystem : SystemBase
{
    EntityQuery foodQuery;

    protected override void OnCreate()
    {
        foodQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(FoodData), typeof(Translation), typeof(ResourceIsAvailableTag) }
        });
    }

    protected override void OnUpdate()
    {
        if (foodQuery.CalculateChunkCount() == 0)
            return;

        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        int foodAmount = foodQuery.CalculateEntityCount();

        Entities.WithNone<IsHungryTag, MovingToEatFoodData>().WithAll<IdleTag>().ForEach((Entity entity, ref CitizenFoodData citizenFoodData) =>
        {
            if (citizenFoodData.CurrentFoodLevel < 10 && foodAmount > 0)
            {
                // Citizen is hungry
                CommandBuffer.AddComponent<IsHungryTag>(entity);
                CommandBuffer.RemoveComponent<IdleTag>(entity);
                foodAmount--;
            }
        }).Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }
}
