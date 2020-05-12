using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class CitizenFamilyHouseAssignmentSystem : SystemBase
{
    EntityQuery housesQuery;
    EntityQuery familyQuery;

    protected override void OnCreate()
    {
        housesQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(HouseData), typeof(CitizenElement), typeof(Translation) }
        });

        familyQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(FamilyData) }
        });
    }

    protected override void OnUpdate()
    {
        if (familyQuery.CalculateEntityCount() == 0 || housesQuery.CalculateEntityCount() == 0)
            return;

        var CommandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        NativeArray<Entity> familyEntites = familyQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<FamilyData> familyDatas = familyQuery.ToComponentDataArray<FamilyData>(Allocator.TempJob);

        Entities.ForEach((Entity entity, DynamicBuffer<CitizenElement> citizenElements, ref HouseData houseData, ref Translation translation) =>
        {
            if (houseData.FamilyEntity == Entity.Null)
            {
                for (int i = 0; i < familyDatas.Length; i++)
                {
                    if (i >= familyDatas.Length || houseData.FamilyEntity != Entity.Null)
                        break;
                    if (familyDatas[i].HasHome)
                        continue;

                    houseData.FamilyEntity = familyEntites[i];

                    var family = familyDatas[i];

                    if (family.Husband != Entity.Null)
                    {
                        var houseAssignmentEntity = CommandBuffer.CreateEntity();
                        CommandBuffer.AddComponent<AddCitizenToHouseData>(houseAssignmentEntity);
                        CommandBuffer.SetComponent(houseAssignmentEntity, new AddCitizenToHouseData { CitizenEntity = family.Husband, HouseEntity = entity });
                    }
                    if (family.Wife != Entity.Null)
                    {
                        var houseAssignmentEntity = CommandBuffer.CreateEntity();
                        CommandBuffer.AddComponent<AddCitizenToHouseData>(houseAssignmentEntity);
                        CommandBuffer.SetComponent(houseAssignmentEntity, new AddCitizenToHouseData { CitizenEntity = family.Wife, HouseEntity = entity });
                    }

                    family.HasHome = true;
                    familyDatas[i] = family;
                    CommandBuffer.SetComponent(familyEntites[i], family);
                }
            }
        }).Run();

        CommandBuffer.Playback(EntityManager);
        CommandBuffer.Dispose();

        familyEntites.Dispose();
        familyDatas.Dispose();
    }
}