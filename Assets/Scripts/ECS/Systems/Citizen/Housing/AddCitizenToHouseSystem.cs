using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;

public class AddCitizenToHouseSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery addCitizenTohouseQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        addCitizenTohouseQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(AddCitizenToHouseData) },
        });
    }

    protected override void OnUpdate()
    {
        if (addCitizenTohouseQuery.CalculateEntityCount() == 0)
            return;

        var CommandBuffer = bufferSystem.CreateCommandBuffer();

        var entities = addCitizenTohouseQuery.ToEntityArray(Allocator.TempJob);
        var datas = addCitizenTohouseQuery.ToComponentDataArray<AddCitizenToHouseData>(Allocator.TempJob);

        Entities.ForEach((Entity entity, DynamicBuffer<CitizenElement> citizenElements, ref HouseData houseData, ref Translation translation) =>
        {
            for (int i = 0; i < datas.Length; i++)
            {
                if (datas[i].HouseEntity == entity)
                {
                    CommandBuffer.AddComponent<CitizenHousingData>(datas[i].CitizenEntity);
                    CommandBuffer.SetComponent(datas[i].CitizenEntity, new CitizenHousingData
                    {
                        HouseEntity = entity,
                        HousePosition = translation.Value
                    });

                    citizenElements.Add(datas[i].CitizenEntity);
                    houseData.CurrentResidents++;

                    CommandBuffer.DestroyEntity(entities[i]);
                    break;
                }
            }
        }).Schedule(Dependency).Complete();

        entities.Dispose();
        datas.Dispose();
    }
}
