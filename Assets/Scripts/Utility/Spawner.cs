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

                for (int i = 0; i < amountToSpawn - 1; i++)
                {
                    var entity = EntityPrefabManager.Instance.SpawnCitizenPrefab();
                    entityManager.SetComponentData(entity, new Translation { Value = mouseWorldPosition });
                }
            }
            else
            {
                for (int i = 0; i < amountToSpawn; i++)
                {
                    Instantiate(prefabToSpawn);
                }
            }
        }
    }
}
