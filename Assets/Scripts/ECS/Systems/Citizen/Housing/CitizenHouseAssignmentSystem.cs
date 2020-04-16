using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class CitizenHouseAssignmentSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery citizensWithoutHousingQuery;
    EntityQuery housesQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        citizensWithoutHousingQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen) },
            None = new ComponentType[] { typeof(CitizenHousingData) }
        });

        housesQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(HouseData) }
        });
    }

    protected override void OnUpdate()
    {
        if (citizensWithoutHousingQuery.CalculateEntityCount() == 0)
            return;

        var CommandBuffer = bufferSystem.CreateCommandBuffer();

        NativeArray<Entity> citizenEntities = citizensWithoutHousingQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Citizen> citizenDatas = citizensWithoutHousingQuery.ToComponentDataArray<Citizen>(Allocator.TempJob);

        int citizenIndex = 0;
        Entities.ForEach((Entity entity, DynamicBuffer<CitizenElement> citizenElements, ref HouseData houseData, ref Translation translation) =>
        {
            if (houseData.CurrentResidents >= houseData.MaxResidents)
                return;
            if (citizenIndex >= citizenEntities.Length)
                return;

            for (int i = citizenIndex; i < citizenEntities.Length; i++)
            {
                if (houseData.CurrentResidents >= houseData.MaxResidents)
                    break;

                var citizenToAssign = citizenEntities[i];

                CommandBuffer.AddComponent<CitizenHousingData>(citizenToAssign);
                CommandBuffer.SetComponent(citizenToAssign, new CitizenHousingData
                {
                    HouseEntity = entity,
                    HousePosition = translation.Value
                });

                citizenElements.Add(new CitizenElement { Value = citizenDatas[i] });

                houseData.CurrentResidents++;

                citizenIndex++;
            }

        }).Run();


        CommandBuffer.Playback(EntityManager);
        CommandBuffer.ShouldPlayback = false;

        citizenEntities.Dispose();
        citizenDatas.Dispose();
    }
}