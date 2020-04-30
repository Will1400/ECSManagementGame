using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

public class CitizenPregnancyCreationSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery familyQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        familyQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(FamilyData) }
        });
    }

    protected override void OnUpdate()
    {
        if (Random.Range(0, 100) != 0)
            return;

        var CommandBuffer = bufferSystem.CreateCommandBuffer();

        NativeArray<Entity> familyEntites = familyQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<FamilyData> familyDatas = familyQuery.ToComponentDataArray<FamilyData>(Allocator.TempJob);

        Entities.WithNone<CitizenPregnancyData>().ForEach((Entity entity, ref Citizen citizen, ref CitizenFamily citizenFamily) =>
        {
            if (citizen.CitizenPersonalData.Gender == Gender.Female)
            {
                for (int i = 0; i < familyEntites.Length; i++)
                {
                    if (familyEntites[i] == citizenFamily.FamilyEntity)
                    {
                        if (familyDatas[i].HasHome && Random.Range(0, 10 * familyDatas[i].ChildCount) == 0)
                        {
                            CommandBuffer.AddComponent<CitizenPregnancyData>(entity);
                            CommandBuffer.SetComponent(entity, new CitizenPregnancyData
                            {
                                TimeRemaining = 60,
                                Mother = entity,
                                Father = familyDatas[i].Husband
                            });
                        }
                        break;
                    }
                }
            }
        }).Run();

        familyEntites.Dispose();
        familyDatas.Dispose();
    }
}
