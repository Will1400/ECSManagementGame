using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class FoodDepletionSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltatime = Time.DeltaTime;

        Entities.ForEach((Entity entity, ref CitizenFoodData foodData) =>
        {
            foodData.CurrentFoodLevel -= deltatime / 10 * foodData.DepletionMultiplier;
        }).Schedule();
    }
}