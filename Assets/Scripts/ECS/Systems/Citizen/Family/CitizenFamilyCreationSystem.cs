﻿using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;

public class CitizenFamilyCreationSystem : SystemBase
{
    EntityQuery citizensWithoutFamilyQuery;

    protected override void OnCreate()
    {
        citizensWithoutFamilyQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen) },
            None = new ComponentType[] { typeof(IsChildTag), typeof(CitizenFamily) }
        });
    }

    protected override void OnUpdate()
    {
        if (citizensWithoutFamilyQuery.CalculateEntityCount() == 0)
            return;

        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        var citizenEntites = citizensWithoutFamilyQuery.ToEntityArray(Allocator.TempJob);
        var citizenDatas = citizensWithoutFamilyQuery.ToComponentDataArray<Citizen>(Allocator.TempJob);

        int maleIndex = 0;
        int femaleIndex = 0;

        while (maleIndex < citizenEntites.Length)
        {
            var family = new FamilyData { };

            for (int m = maleIndex; m < citizenDatas.Length; m++)
            {
                if (citizenDatas[m].CitizenPersonalData.Gender == Gender.Male)
                {
                    family.Husband = citizenEntites[m];
                    maleIndex++;
                    break;
                }
                maleIndex++;
            }

            for (int f = femaleIndex; f < citizenDatas.Length; f++)
            {
                if (citizenDatas[f].CitizenPersonalData.Gender == Gender.Female)
                {
                    family.Wife = citizenEntites[f];
                    femaleIndex++;
                    break;
                }
                femaleIndex++;
            }

            if (family.Husband != Entity.Null && family.Wife != Entity.Null)
            {
                var entity = CommandBuffer.CreateEntity();
                CommandBuffer.AddComponent<FamilyData>(entity);
                CommandBuffer.SetComponent(entity, family);

                CommandBuffer.AddComponent<CitizenFamily>(family.Husband);
                CommandBuffer.SetComponent(family.Husband, new CitizenFamily { FamilyEntity = entity });

                CommandBuffer.AddComponent<CitizenFamily>(family.Wife);
                CommandBuffer.SetComponent(family.Wife, new CitizenFamily { FamilyEntity = entity });
            }
        }

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();

        citizenEntites.Dispose();
        citizenDatas.Dispose();
    }
}
