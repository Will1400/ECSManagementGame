using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

[UpdateInGroup(typeof(MovementGroup))]
public class NavAgentMovementSystem : SystemBase
{
    EndSimulationEntityCommandBufferSystem bufferSystem;
    EntityQuery navAgentsToMoveQuery;

    protected override void OnCreate()
    {
        bufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        navAgentsToMoveQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(MoveSpeed), typeof(Translation), typeof(NavAgent), typeof(NavAgentPathPointElement), typeof(NavAgentHasPathTag) },
            None = new ComponentType[] { typeof(HasArrivedAtDestinationTag) }
        });
    }

    protected override void OnUpdate()
    {
        var job = new ChunkMoveJob
        {
            EntityType = GetArchetypeChunkEntityType(),
            TranslationType = GetArchetypeChunkComponentType<Translation>(),
            RotationType = GetArchetypeChunkComponentType<Rotation>(),
            NavAgentType = GetArchetypeChunkComponentType<NavAgent>(),
            BufferElementType = GetArchetypeChunkBufferType<NavAgentPathPointElement>(true),
            MoveSpeedType = GetArchetypeChunkComponentType<MoveSpeed>(true),

            DeltaTime = Time.DeltaTime,
            CommandBuffer = bufferSystem.CreateCommandBuffer().ToConcurrent()
        }.Schedule(navAgentsToMoveQuery);

        job.Complete();
    }

    [BurstCompile]
    struct ChunkMoveJob : IJobChunk
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        [ReadOnly]
        public float DeltaTime;

        [ReadOnly]
        public ArchetypeChunkEntityType EntityType;
        public ArchetypeChunkComponentType<Translation> TranslationType;
        public ArchetypeChunkComponentType<Rotation> RotationType;
        public ArchetypeChunkComponentType<NavAgent> NavAgentType;
        [ReadOnly]
        public ArchetypeChunkBufferType<NavAgentPathPointElement> BufferElementType;
        [ReadOnly]
        public ArchetypeChunkComponentType<MoveSpeed> MoveSpeedType;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<Entity> entities = chunk.GetNativeArray(EntityType);

            NativeArray<Translation> translations = chunk.GetNativeArray(TranslationType);
            NativeArray<Rotation> rotations = chunk.GetNativeArray(RotationType);
            NativeArray<NavAgent> navAgents = chunk.GetNativeArray(NavAgentType);
            NativeArray<MoveSpeed> moveSpeeds = chunk.GetNativeArray(MoveSpeedType);
            BufferAccessor<NavAgentPathPointElement> buffers = chunk.GetBufferAccessor(BufferElementType);

            for (int i = 0; i < chunk.Count; i++)
            {
                var buffer = buffers[i];
                var translation = translations[i];
                var rotation = rotations[i];
                var agent = navAgents[i];

                if (agent.CurrentWaypointIndex >= buffer.Length && buffer.Length > 0)
                {
                    agent.Status = AgentStatus.Idle;
                    navAgents[i] = agent;
                    CommandBuffer.RemoveComponent<NavAgentHasPathTag>(chunkIndex, entities[i]);
                    CommandBuffer.AddComponent<HasArrivedAtDestinationTag>(chunkIndex, entities[i]);
                    break;
                }

                float3 destination = buffer[agent.CurrentWaypointIndex];
                destination.y = translation.Value.y;

                if (math.distance(translation.Value, destination) > .4f)
                {
                    float3 direction = math.normalize(destination - translation.Value);
                    direction.y = 0;
                    rotation.Value = quaternion.LookRotation(direction, new float3(0, 1, 0));
                    translation.Value += direction * moveSpeeds[i].Value * DeltaTime;

                    translations[i] = translation;
                    rotations[i] = rotation;
                }
                else
                {
                    agent.CurrentWaypointIndex++;
                    navAgents[i] = agent;
                }
            }
        }
    }
}
