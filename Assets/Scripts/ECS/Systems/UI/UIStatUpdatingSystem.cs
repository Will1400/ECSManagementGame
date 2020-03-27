using UnityEngine;
using System.Collections;
using Unity.Entities;
using UnityEngine.Events;
using System;
using Unity.Collections;
using System.Linq;
using Unity.Burst;

public class UIStatUpdatingSystem : SystemBase
{
    public static UIStatUpdatingSystem Instance;

    public Action<int> CitizenCountChanged;
    public Action<int> StoneResourceCountChanged;

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

    private int stoneResourceCount;
    public int StoneResourceCount
    {
        get { return stoneResourceCount; }
        set
        {
            if (value != stoneResourceCount)
            {
                StoneResourceCountChanged?.Invoke(value);
                stoneResourceCount = value;
            }
        }
    }


    EntityQuery citizensQuery;
    EntityQuery resourcesQuery;

    protected override void OnCreate()
    {
        numOfResourceTypes = Enum.GetValues(typeof(ResourceType)).Length;

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


        NativeQueue<ResourceCountInfo> resourceCountInfoQueue = new NativeQueue<ResourceCountInfo>(Allocator.TempJob);


        var countJob = new ResourceBasicCountJob
        {
            ResourceDataType = GetArchetypeChunkComponentType<ResourceData>(true),
            CountedResources = resourceCountInfoQueue.AsParallelWriter(),
            NumOfResourceTypes = numOfResourceTypes
        }.Schedule(resourcesQuery);
        countJob.Complete();


        NativeArray<int> resourceCounts = new NativeArray<int>(numOfResourceTypes, Allocator.Temp);

        while (resourceCountInfoQueue.TryDequeue(out ResourceCountInfo countInfo))
        {
            resourceCounts[(int)countInfo.ResourceType] += countInfo.Count;
        }

        for (int i = 0; i < resourceCounts.Length; i++)
        {
            if ((ResourceType)i == ResourceType.Stone)
            {
                StoneResourceCount = resourceCounts[i];
            }
        }

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
