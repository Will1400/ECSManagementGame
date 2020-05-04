using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[UpdateBefore(typeof(WorkAssignmentGroup))]
public class CitizenFindFoodWhenHungrySystem : SystemBase
{
    protected override void OnUpdate()
    {
        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        Entities.WithNone<IsHungryTag, MovingToEatFoodData>().ForEach((Entity entity, ref CitizenFoodData citizenFoodData) =>
        {
            if (citizenFoodData.CurrentFoodLevel < 10)
            {
                // Citizen is hungry
                CommandBuffer.AddComponent<IsHungryTag>(entity);
                CommandBuffer.RemoveComponent<IdleTag>(entity);
            }
        }).Schedule(Dependency).Complete();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }
}
