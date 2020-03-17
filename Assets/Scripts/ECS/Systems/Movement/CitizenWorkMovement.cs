using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

[DisableAutoCreation]
public class CitizenWorkMovement : JobComponentSystem
{

    EntityQuery toMoveQuery;

    protected override void OnCreate()
    {
        toMoveQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] {typeof(CitizenWork), typeof(MoveSpeed), typeof(Translation) }
        });
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var moveJob = new MoveChunkJob
        {
            TranslationType = GetArchetypeChunkComponentType<Translation>(),
            CitizenWorkType = GetArchetypeChunkComponentType<CitizenWork>(true),
            MoveSpeedType = GetArchetypeChunkComponentType<MoveSpeed>(true),
            DeltaTime = Time.DeltaTime
        };

        return moveJob.Schedule(toMoveQuery, inputDeps);
    }


    struct MoveChunkJob : IJobChunk
    {
        public ArchetypeChunkComponentType<Translation> TranslationType;
        [ReadOnly]
        public ArchetypeChunkComponentType<MoveSpeed> MoveSpeedType;
        [ReadOnly]
        public ArchetypeChunkComponentType<CitizenWork> CitizenWorkType;

        [ReadOnly]
        public float DeltaTime;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Translation> translations = chunk.GetNativeArray(TranslationType);
            NativeArray<CitizenWork> citizenWorks = chunk.GetNativeArray(CitizenWorkType);
            NativeArray<MoveSpeed> moveSpeeds = chunk.GetNativeArray(MoveSpeedType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var translation = translations[i];
                var work = citizenWorks[i];
                var speed = moveSpeeds[i];

                if (math.distance(translation.Value, work.WorkPosition) >= .2f)
                {
                    float3 direction = math.normalize(work.WorkPosition - translation.Value);
                    direction.y = 0;
                    translation.Value += direction * speed.Value * DeltaTime;
                }
            }
        }
    }

    //[BurstCompile]
    //[RequireComponentTag(typeof(GoingToWorkTag))]
    //private struct MoveJob : IJobForEachWithEntity<CitizenWork, MoveSpeed, Translation, Rotation>
    //{
    //    [ReadOnly]
    //    public float deltatime;

    //    public void Execute(Entity entity, int index, [ReadOnly] ref CitizenWork work, [ReadOnly] ref MoveSpeed moveSpeed, ref Translation translation, ref Rotation rotation)
    //    {
    //        if (math.distance(translation.Value, work.WorkPosition) >= .2f)
    //        {
    //            float3 direction = math.normalize(work.WorkPosition - translation.Value);
    //            direction.y = 0;
    //            translation.Value += direction * moveSpeed.Value * deltatime;
    //        }
    //    }
    //}
}
