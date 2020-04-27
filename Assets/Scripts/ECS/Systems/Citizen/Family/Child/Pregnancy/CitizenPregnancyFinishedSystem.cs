using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;

public class CitizenPregnancyFinishedSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        Entities.ForEach((Entity entity, ref CitizenPregnancyData pregnancyData, ref Translation translation, ref CitizenFamily citizenFamily) =>
        {
            if (pregnancyData.TimeRemaining <= 0)
            {
                var child = EntityPrefabManager.Instance.SpawnCitizenPrefab(CommandBuffer);

                CommandBuffer.SetComponent(child, translation);

                // Add to family
                CommandBuffer.AddComponent<CitizenFamily>(child);
                CommandBuffer.SetComponent(child, new CitizenFamily { FamilyEntity = citizenFamily.FamilyEntity });

                var familyData = EntityManager.GetComponentData<FamilyData>(citizenFamily.FamilyEntity);
                familyData.ChildCount++;
                CommandBuffer.SetComponent(citizenFamily.FamilyEntity, familyData);

                // Add to house
                var houseAssignmentEntity = CommandBuffer.CreateEntity();
                CommandBuffer.AddComponent<AddCitizenToHouseData>(houseAssignmentEntity);
                CommandBuffer.SetComponent(houseAssignmentEntity, new AddCitizenToHouseData { CitizenEntity = child, HouseEntity = EntityManager.GetComponentData<CitizenHousingData>(entity).HouseEntity });

                CommandBuffer.RemoveComponent<CitizenPregnancyData>(entity);
            }
        }).WithoutBurst().Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }
}
