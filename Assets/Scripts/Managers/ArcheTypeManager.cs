using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System;

public class ArcheTypeManager : MonoBehaviour
{
    public static ArcheTypeManager Instance;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);

        ArcheTypes = new Dictionary<PredifinedArchetype, EntityArchetype>();
    }

    public Dictionary<PredifinedArchetype, EntityArchetype> ArcheTypes;

    EntityManager entityManager;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        CreateArcheTypes();
    }

    void CreateArcheTypes()
    {
        ArcheTypes.Add(PredifinedArchetype.ConstructionSite, entityManager.CreateArchetype(
            typeof(UnderConstruction),
            typeof(WorkPlaceWorkerData),
            typeof(Translation),
            typeof(GridOccupation)));

    

        ArcheTypes.Add(PredifinedArchetype.BeingPlaced, entityManager.CreateArchetype(
            typeof(BeingPlacedTag),
            typeof(GridOccupation),
            typeof(Translation),
            typeof(Rotation),
            typeof(Scale),
            typeof(RenderMesh),
            typeof(WorldRenderBounds),
            typeof(RenderBounds),
            typeof(LocalToWorld)));
    }

    public EntityArchetype GetArcheType(PredifinedArchetype archetype)
    {
        return ArcheTypes[archetype];
    }
}
