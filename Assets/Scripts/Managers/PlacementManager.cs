using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.Entities;
using Unity.Mathematics;
using System;
using Unity.Rendering;
using Unity.Collections;
using Unity.Transforms;

public class PlacementManager : MonoBehaviour
{

    public static PlacementManager Instance;

    private uint collisionLayerMask;
    [SerializeField]
    private GameObject currentObject;
    private float3 currentObjectOffset;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);
        collisionLayerMask = 1u << 9;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (currentObject != null)
                SpawnBuilding(PrefabManager.Instance.GetBuilding(1).name);
            else
                SpawnBuilding("TallHouse");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnUnit("Citizen");
        }

        if (GameManager.Instance.CursorState == CursorState.Building)
        {
            if (Input.GetButton("Cancel") || Input.GetButton("Secondary Mouse"))
                CancelBuild();

            MoveBuildingToMouse();

            if (Input.GetKeyDown(KeyCode.R))
                RotateBuilding();

            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
                PlaceCurrentObject();
        }
    }

    private void RotateBuilding()
    {
        currentObject.transform.Rotate(new Vector3(0, 90));
    }

    public void SpawnUnit(string unitName)
    {
        CancelBuild();

        Instantiate(Resources.Load<GameObject>("Prefabs/Citizen"));
    }

    public void SpawnBuilding(string buildingName)
    {
        CancelBuild();
        currentObject = Instantiate(PrefabManager.Instance.GetBuilding(buildingName));
        currentObjectOffset = currentObject.transform.position;
        GameManager.Instance.CursorState = CursorState.Building;
    }

    void MoveBuildingToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var hit = ECSRaycast.Raycast(ray.origin, ray.direction * 999, collisionLayerMask);
        currentObject.transform.position = hit.Position + currentObjectOffset;
    }

    void PlaceCurrentObject()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var constructionEntity = entityManager.CreateEntity(typeof(UnderConstruction),
                                                            typeof(NeedsWorkers),
                                                            typeof(Translation));

        entityManager.AddComponentData(constructionEntity, new Translation { Value = currentObject.transform.position });
        entityManager.AddComponentData(constructionEntity, new NeedsWorkers { WorkersNeeded = 3, WorkPosition = currentObject.transform.position });
        entityManager.AddComponentData(constructionEntity, new UnderConstruction { totalConstructionTime = 4, remainingConstructionTime = 4, maxWorkers = 3, currentWorkers = 0, finishedPrefabName = currentObject.name });

        CancelBuild();
        GameManager.Instance.CursorState = CursorState.None;
    }

    void CancelBuild()
    {
        if (currentObject == null)
            return;

        Destroy(currentObject);
        currentObject = null;
        GameManager.Instance.CursorState = CursorState.None;
    }
}
