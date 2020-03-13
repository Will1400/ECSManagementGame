using UnityEngine;
using System.Collections;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Entities;
using Unity.Mathematics;

public class Spawner : MonoBehaviour
{
    [SerializeField]
    GameObject prefabToSpawn;
    [SerializeField]
    int amountToSpawn;

    public Material Material;
    public Mesh Mesh;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (prefabToSpawn.name == "Citizen")
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float3 mouseWorldPosition = ECSRaycast.Raycast(ray.origin, ray.direction * 999, 1u << 9).Position;
                mouseWorldPosition.y = 1.5f;
                var entity = ArcheTypeManager.Instance.GetSetupCitizenEntity(mouseWorldPosition);
                for (int i = 0; i < amountToSpawn - 1; i++)
                {
                    entityManager.Instantiate(entity);
                }
            }
            else
            {
                for (int i = 0; i < amountToSpawn; i++)
                {
                    Instantiate(prefabToSpawn);

                    //GameObject prefab = PrefabManager.Instance.GetBuilding(1);

                    //Mesh prefabMesh = prefab.GetComponent<MeshFilter>().sharedMesh;
                    //Renderer renderer = prefab.GetComponent<Renderer>();
                    //Mesh = prefabMesh;
                    //Material = renderer.sharedMaterial;

                    //EntityArchetype type = entityManager.CreateArchetype(typeof(Translation),
                    //                              typeof(Rotation),
                    //                              typeof(Scale),
                    //                              typeof(RenderMesh),
                    //                              typeof(RenderBounds),
                    //                              typeof(LocalToWorld));

                    //var entity = entityManager.CreateEntity(type);

                    //entityManager.AddSharedComponentData(entity, new RenderMesh { mesh = Mesh, material = Material });
                    //entityManager.AddComponentData(entity, new RenderBounds { Value = Mesh.bounds.ToAABB() });
                    //entityManager.AddComponentData(entity, new Scale { Value = prefab.transform.localScale.x });
                    //entityManager.AddComponentData(entity, new Rotation { Value = prefab.transform.rotation });

                }
            }
        }
    }
}
