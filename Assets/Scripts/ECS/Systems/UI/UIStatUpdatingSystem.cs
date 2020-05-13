using UnityEngine;
using System.Collections;
using Unity.Entities;
using UnityEngine.Events;
using System;
using Unity.Collections;
using Unity.Burst;

[UpdateInGroup(typeof(UIGroup))]
public class UIStatUpdatingSystem : SystemBase
{
    public static UIStatUpdatingSystem Instance;

    public Action<int> CitizenCountChanged;
    public Action<ResourceType, int> ResourceCountChanged;

    NativeArray<int> resourceCounts;
    NativeArray<int> newResourceCounts;
    NativeQueue<ResourceCountInfo> resourceCountInfoQueue;

    int numOfResourceTypes;

    private int citizenCount;

    public int CitizenCount
    {
        get { return citizenCount; }
        set
        {
            if (value != citizenCount)
            {
                CitizenCountChanged?.Invoke(value);
                citizenCount = value;
            }
        }
    }

    EntityQuery citizensQuery;
    EntityQuery resourcesQuery;

    protected override void OnCreate()
    {
        numOfResourceTypes = Enum.GetValues(typeof(ResourceType)).Length;

        resourceCounts = new NativeArray<int>(numOfResourceTypes, Allocator.Persistent);
        newResourceCounts = new NativeArray<int>(numOfResourceTypes, Allocator.Persistent);
        resourceCountInfoQueue = new NativeQueue<ResourceCountInfo>(Allocator.Persistent);

        if (Instance == null)
            Instance = this;

        citizensQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen) }
        });

        resourcesQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(ResourceData) }
        });
    }

    protected override void OnUpdate()
    {
        CitizenCount = citizensQuery.CalculateEntityCount();


        ResetNewCount();

        var countJob = new ResourceBasicCountJob
        {
            ResourceDataType = GetArchetypeChunkComponentType<ResourceData>(true),
            CountedResources = resourceCountInfoQueue.AsParallelWriter(),
            NumOfResourceTypes = numOfResourceTypes
        }.Schedule(resourcesQuery);
        countJob.Complete();

        while (resourceCountInfoQueue.TryDequeue(out ResourceCountInfo countInfo))
        {
            newResourceCounts[(int)countInfo.ResourceType] += countInfo.Count;
        }

        for (int i = 0; i < resourceCounts.Length; i++)
        {
            if (resourceCounts[i] != newResourceCounts[i])
            {
                resourceCounts[i] = newResourceCounts[i];
                ResourceCountChanged?.Invoke((ResourceType)i, resourceCounts[i]);
            }
        }
    }

    void ResetNewCount()
    {
        for (int i = 0; i < newResourceCounts.Length; i++)
        {
            newResourceCounts[i] = 0;
        }
    }

    protected override void OnDestroy()
    {
        resourceCounts.Dispose();
        newResourceCounts.Dispose();
        resourceCountInfoQueue.Dispose();
    }

    [BurstCompile]
    struct ResourceBasicCountJob : IJobChunk
    {
        [ReadOnly]
        public ArchetypeChunkComponentType<ResourceData> ResourceDataType;

        [ReadOnly]
        public int NumOfResourceTypes;

        /// <summary>
        /// Key is a ResourceType, value is the counted
        /// </summary>
        public NativeQueue<ResourceCountInfo>.ParallelWriter CountedResources;

        public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
        {
            NativeArray<ResourceData> resourceDatas = chunk.GetNativeArray(ResourceDataType);

            NativeArray<int> resourceCounts = new NativeArray<int>(NumOfResourceTypes, Allocator.Temp);

            for (int i = 0; i < chunk.Count; i++)
            {
                resourceCounts[(int)resourceDatas[i].ResourceType] += resourceDatas[i].Amount;
            }

            for (int i = 0; i < NumOfResourceTypes; i++)
            {
                CountedResources.Enqueue(new ResourceCountInfo { ResourceType = (ResourceType)i, Count = resourceCounts[i] });
            }

            resourceCounts.Dispose();
        }
    }

    struct ResourceCountInfo
    {
        public ResourceType ResourceType;
        public int Count;
    }
}
