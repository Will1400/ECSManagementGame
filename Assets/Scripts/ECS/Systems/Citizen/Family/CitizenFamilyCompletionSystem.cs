using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;

[DisableAutoCreation]
public class CitizenFamilyCompletionSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery citizensQuery;
    EntityQuery familyQuery;

    NativeQueue<IncompleteFamily> familiesNeedingHusbands;
    NativeQueue<IncompleteFamily> familiesNeedingWifes;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        citizensQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen) },
            None = new ComponentType[] { typeof(IsChildTag), typeof(CitizenFamily) }
        });

        familyQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(FamilyData) }
        });

        familiesNeedingHusbands = new NativeQueue<IncompleteFamily>(Allocator.Persistent);
        familiesNeedingWifes = new NativeQueue<IncompleteFamily>(Allocator.Persistent);
    }

    protected override void OnUpdate()
    {
        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        //var missingHusbands = familiesNeedingHusbands.AsParallelWriter();
        //var missingWifes = familiesNeedingWifes.AsParallelWriter();
        Entities.ForEach((Entity entity, ref FamilyData familyData) =>
        {
            if (familyData.Husband == Entity.Null)
            {
                familiesNeedingHusbands.Enqueue(new IncompleteFamily { FamilyData = familyData, FamilyEntity = entity });
            }
            if (familyData.Wife == Entity.Null)
            {
                familiesNeedingWifes.Enqueue(new IncompleteFamily { FamilyData = familyData, FamilyEntity = entity });
            }
        }).WithoutBurst().Run();


        if (familiesNeedingHusbands.Count > 0 || familiesNeedingWifes.Count > 0)
        {
            NativeArray<Entity> citizenEntites = citizensQuery.ToEntityArray(Allocator.TempJob);
            NativeArray<Citizen> citizenDatas = citizensQuery.ToComponentDataArray<Citizen>(Allocator.TempJob);

            int citizenIndex = 0;
            while (familiesNeedingHusbands.TryDequeue(out IncompleteFamily incompleteFamily))
            {
                if (citizenIndex >= citizenEntites.Length)
                    continue;

                for (int i = citizenIndex; i < citizenDatas.Length; i++)
                {
                    if (citizenIndex >= citizenEntites.Length)
                        break;
                    if (citizenDatas[i].CitizenPersonalData.Gender != Gender.Male)
                        continue;

                    incompleteFamily.FamilyData.Husband = citizenEntites[i];
                    CommandBuffer.AddComponent<CitizenFamily>(citizenEntites[i]);
                    CommandBuffer.SetComponent(incompleteFamily.FamilyEntity, incompleteFamily.FamilyData);

                    citizenIndex++;
                }
            }

            citizenIndex = 0;
            while (familiesNeedingWifes.TryDequeue(out IncompleteFamily incompleteFamily))
            {
                if (citizenIndex >= citizenEntites.Length)
                    continue;

                for (int i = citizenIndex; i < citizenDatas.Length; i++)
                {
                    if (citizenIndex >= citizenEntites.Length)
                        break;
                    if (citizenDatas[i].CitizenPersonalData.Gender != Gender.Female)
                        continue;

                    incompleteFamily.FamilyData.Wife = citizenEntites[i];
                    CommandBuffer.AddComponent<CitizenFamily>(citizenEntites[i]);
                    CommandBuffer.SetComponent(incompleteFamily.FamilyEntity, incompleteFamily.FamilyData);

                    citizenIndex++;
                }
            }

            CommandBuffer.Playback(EntityManager);
            CommandBuffer.ShouldPlayback = false;

            citizenEntites.Dispose();
            citizenDatas.Dispose();
        }
    }

    protected override void OnDestroy()
    {
        familiesNeedingHusbands.Dispose();
        familiesNeedingWifes.Dispose();
    }

    struct IncompleteFamily
    {
        public FamilyData FamilyData;
        public Entity FamilyEntity;
    }
}
