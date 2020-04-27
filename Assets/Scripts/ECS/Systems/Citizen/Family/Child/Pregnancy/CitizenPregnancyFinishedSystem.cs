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
                var child = EntityPrefabManager.Instance.SpawnCitizenPrefab();

                EntityManager.SetComponentData(child, translation);

                var familyData = EntityManager.GetComponentData<FamilyData>(citizenFamily.FamilyEntity);

                // Add to family
                EntityManager.AddComponent<CitizenFamily>(child);
                EntityManager.SetComponentData(child, new CitizenFamily { FamilyEntity = citizenFamily.FamilyEntity });

                familyData.ChildCount++;
                EntityManager.SetComponentData(citizenFamily.FamilyEntity, familyData);

                // Add to house
                var houseAssignmentEntity = CommandBuffer.CreateEntity();
                CommandBuffer.AddComponent<AddCitizenToHouseData>(houseAssignmentEntity);
                CommandBuffer.SetComponent(houseAssignmentEntity, new AddCitizenToHouseData { CitizenEntity = child, HouseEntity = EntityManager.GetComponentData<CitizenHousingData>(entity).HouseEntity });

                CommandBuffer.RemoveComponent<CitizenPregnancyData>(entity);
            }
        }).WithStructuralChanges().WithoutBurst().Run();


        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();
    }
}
