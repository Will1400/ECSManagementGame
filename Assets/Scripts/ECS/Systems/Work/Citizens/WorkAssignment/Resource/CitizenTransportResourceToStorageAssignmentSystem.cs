using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Collections;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Mathematics;

public class CitizenTransportResourceToStorageAssignmentSystem : SystemBase
{
    EntityQuery ResourcesToMoveIntoStorageQuery;
    EntityQuery IdleCitizensQuery;
    EntityQuery StorageAreasQuery;

    protected override void OnCreate()
    {
        ResourcesToMoveIntoStorageQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceData), typeof(TransportResourceToStorageTag), typeof(Translation) },
            None = new ComponentType[] { typeof(ResourceBeingCarriedTag) }
        });

        IdleCitizensQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen), typeof(IdleTag) },
            None = new ComponentType[] { typeof(MovingToPickupResource) }
        });

        StorageAreasQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceStorageArea) },
        });
    }

    protected override void OnUpdate()
    {
        if (ResourcesToMoveIntoStorageQuery.CalculateChunkCount() != 0 && IdleCitizensQuery.CalculateChunkCount() != 0 && StorageAreasQuery.CalculateChunkCount() != 0)
        {
            var idlecitizens = IdleCitizensQuery.ToEntityArray(Allocator.TempJob);
            var storageAreaEntities = StorageAreasQuery.ToEntityArray(Allocator.TempJob);

            NativeQueue<AssignmentInfo> assignmentQueue = new NativeQueue<AssignmentInfo>(Allocator.TempJob);

            int citizenIndex = 0;
            Entities.WithAll<ResourceData, TransportResourceToStorageTag>().ForEach((Entity entity, ref Translation translation) =>
            {
                for (int i = citizenIndex; i < idlecitizens.Length; i++)
                {
                    var citizen = idlecitizens[i];

                    var assignmentInfo = new AssignmentInfo
                    {
                        Citizen = citizen,
                        Resource = entity,
                        ResourcePosition = translation.Value,
                        TransportJob = new ResourceTransportJobData
                        {
                            DestinationEntity = storageAreaEntities[0],
                            ResourceEntity = entity,
                            DestinationPosition = float3.zero
                        }
                    };

                    assignmentQueue.Enqueue(assignmentInfo);
                }
            }).Run();

            while (assignmentQueue.TryDequeue(out AssignmentInfo assignmentInfo))
            {
                if (math.all(assignmentInfo.TransportJob.DestinationPosition == float3.zero))
                {
                    assignmentInfo.TransportJob.DestinationPosition = EntityManager.GetComponentData<Translation>(assignmentInfo.TransportJob.DestinationEntity).Value;
                }

                NavAgentRequestingPath requestingPath = new NavAgentRequestingPath
                {
                    StartPosition = EntityManager.GetComponentData<Translation>(assignmentInfo.Citizen).Value,
                    EndPosition = assignmentInfo.ResourcePosition
                };

                EntityManager.AddComponentData(assignmentInfo.Citizen, new MovingToPickupResource { ResourceEntity = assignmentInfo.Resource });

                EntityManager.AddComponentData(assignmentInfo.Citizen, requestingPath);

                EntityManager.AddComponentData(assignmentInfo.Citizen, assignmentInfo.TransportJob);

                EntityManager.RemoveComponent<IdleTag>(assignmentInfo.Citizen);
            }

            idlecitizens.Dispose();
            assignmentQueue.Dispose();
            storageAreaEntities.Dispose();
        }
    }

    struct AssignmentInfo
    {
        public Entity Citizen;
        public Entity Resource;
        public float3 ResourcePosition;
        public ResourceTransportJobData TransportJob;
    }
}
