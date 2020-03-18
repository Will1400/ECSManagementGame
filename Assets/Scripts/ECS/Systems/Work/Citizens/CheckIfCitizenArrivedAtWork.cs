using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

public class CheckIfCitizenArrivedAtWork : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;
    EntityQuery citizensToCheckQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        citizensToCheckQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(CitizenWork),typeof(Translation), typeof(Citizen), typeof(GoingToWorkTag) }
        });
    }

    protected override void OnUpdate()
    {
        var job = new CheckIfArrivedAtWorkJob
        {
            EntityType = GetArchetypeChunkEntityType(),
            CitizenWorkType = GetArchetypeChunkComponentType<CitizenWork>(true),
            TranslationType = GetArchetypeChunkComponentType<Translation>(true),
            CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(citizensToCheckQuery);

        job.Complete();
    }

    [BurstCompile]
    struct CheckIfArrivedAtWorkJob : IJobChunk
    {
        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;
        [ReadOnly]
        public ArchetypeChunkComponentType<Translation> TranslationType;
        [ReadOnly]
        public ArchetypeChunkComponentType<CitizenWork> CitizenWorkType;

        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            var entities = chunk.GetNativeArray(EntityType);
            var translations = chunk.GetNativeArray(TranslationType);
            var citizenWorks = chunk.GetNativeArray(CitizenWorkType);

            for (int i = 0; i < chunk.Count; i++)
            {
                float3 correctedTranslation = translations[i].Value;
                correctedTranslation.y = citizenWorks[i].WorkPosition.y;
                float distance = math.distance(correctedTranslation, citizenWorks[i].WorkPosition);

                if (distance <= .5f)
                {
                    CommandBuffer.AddComponent<HasArrivedAtWorkTag>(chunkIndex, entities[i]);
                }
            }
        }
    }
}
