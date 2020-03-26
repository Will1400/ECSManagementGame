using UnityEngine;
using System.Collections;
using Unity.Entities;
using UnityEngine.Events;
using System;
using Unity.Collections;
using System.Linq;

public class UIStatUpdatingSystem : SystemBase
{
    public static UIStatUpdatingSystem Instance;

    public Action<int> CitizenCountChanged;
    public Action<int> StoneResourceCountChanged;

    int citizenCount;
    int stoneResourceCount;

    EntityQuery citizensQuery;
    EntityQuery resourcesQuery;

    protected override void OnCreate()
    {
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
        int newCitizenCount = citizensQuery.CalculateEntityCount();

        if (citizenCount != newCitizenCount)
        {
            CitizenCountChanged?.Invoke(newCitizenCount);
            citizenCount = newCitizenCount;
        }

        NativeArray<ResourceData> resources = resourcesQuery.ToComponentDataArray<ResourceData>(Allocator.TempJob);

        int newStoneResourceCount = resources.Where(x => x.ResourceType == ResourceType.Stone).Count();
        if (stoneResourceCount != newStoneResourceCount)
        {
            StoneResourceCountChanged?.Invoke(newStoneResourceCount);
            stoneResourceCount = newStoneResourceCount;
        }

        resources.Dispose();
    }
}
