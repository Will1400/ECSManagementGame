using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

[UpdateInGroup(typeof(MovementGroup))]
public class ResourceTransportMovementSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;

    EntityQuery citizensCarryingResourcesQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        citizensCarryingResourcesQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen), typeof(ResourceTransportJobData), typeof(IsCarryingResourceTag) },
        });
    }

    protected override void OnUpdate()
    {
        var moveJob = new MoveToCarrierJob
        {
            TranslationType = GetArchetypeChunkComponentType<Translation>(),
            RotationType = GetArchetypeChunkComponentType<Rotation>(),
            ResourceTransportJobDataType = GetArchetypeChunkComponentType<ResourceTransportJobData>(),
            CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent()

        }.Schedule(citizensCarryingResourcesQuery);
        moveJob.Complete();
    }

    [BurstCompile]
    struct MoveToCarrierJob : IJobChunk
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public ArchetypeChunkComponentType<Translation> TranslationType;
        public ArchetypeChunkComponentType<Rotation> RotationType;
        public ArchetypeChunkComponentType<ResourceTransportJobData> ResourceTransportJobDataType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Translation> translations = chunk.GetNativeArray(TranslationType);
            NativeArray<Rotation> rotations = chunk.GetNativeArray(RotationType);
            NativeArray<ResourceTransportJobData> transportJobDatas = chunk.GetNativeArray(ResourceTransportJobDataType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var resourceEntity = transportJobDatas[i].ResourceEntity;

                float3 forward = math.forward(rotations[i].Value);

                float3 finalPosition = translations[i].Value + (forward * 1);
                finalPosition.y = 1;

                CommandBuffer.SetComponent(chunkIndex, resourceEntity, new Translation { Value = finalPosition });
                CommandBuffer.SetComponent(chunkIndex, resourceEntity, new Rotation { Value = rotations[i].Value });
            }
        }
    }
}
