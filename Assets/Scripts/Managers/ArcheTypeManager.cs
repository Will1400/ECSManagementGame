using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using UnityEngine.Rendering;

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

        ArcheTypes.Add(PredifinedArchetype.Building, entityManager.CreateArchetype(
            typeof(Translation),
            typeof(Rotation),
            typeof(Scale),
            typeof(RenderMesh),
            typeof(RenderBounds),
            typeof(WorldRenderBounds),
            typeof(LocalToWorld),
            typeof(GridOccupation),
            typeof(NavMeshObstacle)));

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

        ArcheTypes.Add(PredifinedArchetype.Citizen, entityManager.CreateArchetype(
           typeof(Translation),
           typeof(Rotation),
           typeof(Scale),
           typeof(RenderMesh),
           typeof(RenderBounds),
           typeof(WorldRenderBounds),
           typeof(LocalToWorld),
           typeof(Citizen),
           typeof(IdleTag),
           typeof(MoveSpeed),
           typeof(NavAgent)));
    }

    public EntityArchetype GetArcheType(PredifinedArchetype archetype)
    {
        return ArcheTypes[archetype];
    }

    public Entity GetSetupBuildingEntity(Mesh mesh, Material material, GameObject prefab)
    {
        Entity entity = entityManager.CreateEntity(GetArcheType(PredifinedArchetype.Building));

        entityManager.AddComponentData(entity, new Scale { Value = prefab.transform.localScale.x });
        entityManager.AddComponentData(entity, new Rotation { Value = prefab.transform.rotation });
        entityManager.AddSharedComponentData(entity, new RenderMesh { mesh = mesh, material = material, castShadows = ShadowCastingMode.On, receiveShadows = true });
        entityManager.AddSharedComponentData(entity, new NavMeshObstacle { Area = 1});
        entityManager.AddComponentData(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

        return entity;
    }

    public Entity GetSetupCitizenEntity(float3 position)
    {
        Entity entity = entityManager.CreateEntity(GetArcheType(PredifinedArchetype.Citizen));
        var prefab = Resources.Load<GameObject>("Prefabs/Citizen");
        var mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
        var material = Resources.Load<Material>("Materials/Citizen/Default");
        entityManager.AddComponentData(entity, new Scale { Value = prefab.transform.localScale.x });
        entityManager.AddComponentData(entity, new Translation { Value = position });
        entityManager.AddComponentData(entity, new Rotation { Value = prefab.transform.rotation });
        entityManager.AddComponentData(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });
        entityManager.AddSharedComponentData(entity, new RenderMesh { mesh = mesh, material = material, castShadows = ShadowCastingMode.On, receiveShadows = true });

        entityManager.AddComponentData(entity, new MoveSpeed { Value = 5 });

        return entity;
    }
}
