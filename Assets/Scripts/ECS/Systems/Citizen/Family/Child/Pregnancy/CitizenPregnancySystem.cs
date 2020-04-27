using UnityEngine;
using System.Collections;
using Unity.Entities;

public class CitizenPregnancySystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltatime = Time.DeltaTime;

        Entities.ForEach((Entity entity, ref CitizenPregnancyData pregnancyData) =>
        {
            pregnancyData.TimeRemaining -= deltatime;
        }).Schedule();
    }
}
