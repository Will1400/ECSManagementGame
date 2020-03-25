using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using UnityEngine.Rendering;
using System;

[Obsolete] 
public class EntityCreationManager : MonoBehaviour
{
    public static EntityCreationManager Instance;

    ArcheTypeManager archeTypeManager;
    EntityManager entityManager;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        archeTypeManager = ArcheTypeManager.Instance;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    //public Entity GetSetupBuildingEntity(Mesh mesh, Material material, float scale, quaternion rotation, float3 position)
    //{
    //    Entity entity = entityManager.CreateEntity(archeTypeManager.GetArcheType(PredifinedArchetype.Building));

    //    SetDefaultData(entity, position, rotation, scale, mesh, material, ShadowCastingMode.On, true);

    //    entityManager.AddComponentData(entity, new NavMeshObstacle { Area = 1 });
    //    entityManager.AddComponentData(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

    //    return entity;
    //}

    //public Entity GetSetupBuildingEntity(GameObject prefab)
    //{
    //    return GetSetupBuildingEntity(prefab.GetComponent<MeshFilter>().sharedMesh, prefab.GetComponent<Renderer>().sharedMaterial, prefab.transform.localScale.x, transform.rotation, transform.position);
    //}

    //public Entity GetSetupCitizenEntity(float3 position)
    //{
    //    Entity entity = entityManager.CreateEntity(archeTypeManager.GetArcheType(PredifinedArchetype.Citizen));
    //    var prefab = Resources.Load<GameObject>("Prefabs/OtherPrefabs/Citizen");
    //    var mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
    //    SetDefaultData(entity, position, prefab.transform.rotation, prefab.transform.localScale.x, mesh, prefab.GetComponent<Renderer>().sharedMaterial, ShadowCastingMode.On, true);

    //    entityManager.AddComponentData(entity, new MoveSpeed { Value = 5 });

    //    return entity;
    //}

    //public Entity GetSetupResourceEntity(ResourceType resourceType, int amount)
    //{
    //    Entity entity = entityManager.CreateEntity(archeTypeManager.GetArcheType(PredifinedArchetype.Resource));
    //    var prefab = PrefabManager.Instance.GetPrefabByName("Stone");
    //    var mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
    //    SetDefaultData(entity, prefab.transform.position, prefab.transform.rotation, prefab.transform.localScale.x, mesh, prefab.GetComponent<Renderer>().sharedMaterial, ShadowCastingMode.On, true);

    //    entityManager.AddComponentData(entity, new ResourceData { ResourceType = resourceType, Amount = amount });

    //    return entity;
    //}

    //public Entity GetSetupResourceProducerEntity(ResourceType resourceType, int amount, float productionTime)
    //{
    //    Entity entity = entityManager.CreateEntity(archeTypeManager.GetArcheType(PredifinedArchetype.Resource));
    //    var prefab = PrefabManager.Instance.GetPrefabByName("Stone");
    //    var mesh = prefab.GetComponent<MeshFilter>().sharedMesh;
    //    SetDefaultData(entity, prefab.transform.position, prefab.transform.rotation, prefab.transform.localScale.x, mesh, prefab.GetComponent<Renderer>().sharedMaterial, ShadowCastingMode.On, true);
    //    entityManager.AddComponentData(entity, new ResourceProductionData { ResourceType = resourceType, AmountPerProduction = amount, ProductionTime = productionTime, ProductionTimeRemaining = productionTime });

    //    return entity;
    //}

    void SetDefaultData(Entity entity, GameObject prefab)
    {
        SetDefaultData(entity, prefab.transform.position, prefab.transform.rotation, prefab.transform.localScale.x, prefab.GetComponent<MeshFilter>().sharedMesh, prefab.GetComponent<Renderer>().sharedMaterial, ShadowCastingMode.On, true);
    }

    void SetDefaultData(Entity entity, float3 position, quaternion rotation, float scale, Mesh mesh, Material material, ShadowCastingMode shadowCastingMode, bool receiveShadows)
    {
        entityManager.AddComponentData(entity, new Scale { Value = scale });
        entityManager.AddComponentData(entity, new Translation { Value = position });
        entityManager.AddComponentData(entity, new Rotation { Value = rotation });
        entityManager.AddComponentData(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });
        entityManager.AddSharedComponentData(entity, new RenderMesh { mesh = mesh, material = material, castShadows = shadowCastingMode, receiveShadows = receiveShadows });
    }
}
