using UnityEngine;
using System.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using UnityEngine.Rendering;

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

    // Use this for initialization
    void Start()
    {
        archeTypeManager = ArcheTypeManager.Instance;
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    public Entity GetSetupBuildingEntity(Mesh mesh, Material material, float scale, quaternion rotation, float3 position)
    {
        Entity entity = entityManager.CreateEntity(archeTypeManager.GetArcheType(PredifinedArchetype.Building));

        entityManager.AddComponentData(entity, new Scale { Value = scale });
        entityManager.AddComponentData(entity, new Rotation { Value = rotation });
        entityManager.AddComponentData(entity, new Translation { Value =  position});
        entityManager.AddSharedComponentData(entity, new RenderMesh { mesh = mesh, material = material, castShadows = ShadowCastingMode.On, receiveShadows = true });
        entityManager.AddComponentData(entity, new NavMeshObstacle { Area = 1 });
        entityManager.AddComponentData(entity, new RenderBounds { Value = mesh.bounds.ToAABB() });

        return entity;
    }

    public Entity GetSetupBuildingEntity(GameObject prefab)
    {
        return GetSetupBuildingEntity(prefab.GetComponent<MeshFilter>().sharedMesh,prefab.GetComponent<Renderer>().sharedMaterial, prefab.transform.localScale.x, transform.rotation, transform.position);
    }

    public Entity GetSetupCitizenEntity(float3 position)
    {
        Entity entity = entityManager.CreateEntity(archeTypeManager.GetArcheType(PredifinedArchetype.Citizen));
        var prefab = Resources.Load<GameObject>("Prefabs/OtherPrefabs/Citizen");
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
