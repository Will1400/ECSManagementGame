using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;

[UpdateAfter(typeof(WorkAssignmentGroup))]
public class CitizenReturnHomeWhenIdleSystem : SystemBase
{
    EntityQuery idleCitizensWithHousingQuery;

    protected override void OnCreate()
    {
        idleCitizensWithHousingQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen), typeof(IdleTag), typeof(CitizenHousingData) },
            None = new ComponentType[] { typeof(IsAtHomeTag), typeof(IsGoingHomeTag), typeof(NavAgentHasPathTag) }
        });
    }

    protected override void OnUpdate()
    {
        var buffer = new EntityCommandBuffer(Allocator.TempJob);

        var job = new HomeJob
        {
            CommandBuffer = buffer.ToConcurrent(),
            EntityType = GetArchetypeChunkEntityType(),
            CitizenHousingDataType = GetArchetypeChunkComponentType<CitizenHousingData>(true),
            TranslationType = GetArchetypeChunkComponentType<Translation>(true)
        }.Schedule(idleCitizensWithHousingQuery);

        job.Complete();

        buffer.Playback(EntityManager);
        buffer.Dispose();
    }

    struct HomeJob : IJobChunk
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;
        [ReadOnly]
        public ArchetypeChunkComponentType<CitizenHousingData> CitizenHousingDataType;
        [ReadOnly]
        public ArchetypeChunkComponentType<Translation> TranslationType;


        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);

            NativeArray<CitizenHousingData> citizenHousingDatas = chunk.GetNativeArray(CitizenHousingDataType);
            NativeArray<Translation> translations = chunk.GetNativeArray(TranslationType);

            for (int i = 0; i < chunk.Count; i++)
            {
                if (math.distance(translations[i].Value, citizenHousingDatas[i].HousePosition) < 1)
                    continue;

                //CommandBuffer.AddComponent<IsGoingHomeTag>(chunkIndex, entities[i]);

                CommandBuffer.AddComponent<NavAgentRequestingPath>(chunkIndex, entities[i]);
                CommandBuffer.SetComponent(chunkIndex, entities[i], new NavAgentRequestingPath
                {
                    StartPosition = translations[i].Value,
                    EndPosition = citizenHousingDatas[i].HousePosition
                });
            }
        }
    }
}
