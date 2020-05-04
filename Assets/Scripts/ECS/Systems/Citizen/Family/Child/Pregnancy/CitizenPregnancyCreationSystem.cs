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
        if (Random.Range(0, 200) != 0)
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
                        if (familyDatas[i].HasHome)
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
        }).Schedule(Dependency).Complete();

        familyEntites.Dispose();
        familyDatas.Dispose();
    }
}
