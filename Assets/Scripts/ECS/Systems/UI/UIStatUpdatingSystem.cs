using UnityEngine;
using System.Collections;
using Unity.Entities;
using UnityEngine.Events;
using System;

public class UIStatUpdatingSystem : SystemBase
{
    public static UIStatUpdatingSystem Instance;

    public Action<int> CitizenCountChanged;

    int citizenCount;
    EntityQuery citizens;

    protected override void OnCreate()
    {
        if (Instance == null)
            Instance = this;

        citizens = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(Citizen) }
        });
    }

    protected override void OnUpdate()
    {
        int newCount = citizens.CalculateEntityCount();

        if (citizenCount != newCount)
        {
            CitizenCountChanged?.Invoke(newCount);
            citizenCount = newCount;
        }
    }
}
