using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Unity.Entities;
using Unity.Mathematics;
using System;

public class PlacementManager : MonoBehaviour
{

    public static PlacementManager Instance;

    private uint collisionLayerMask;
    [SerializeField]
    private GameObject currentObject;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);
        collisionLayerMask = Convert.ToUInt32(LayerMask.GetMask("Terrain"));
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnBuilding("House1v1");
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnUnit("Swordmen");
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
        GameManager.Instance.CursorState = CursorState.Building;
    }

    public void SpawnBuilding(string buildingName)
    {
        CancelBuild();
        currentObject = Instantiate(PrefabManager.Instance.GetBuilding(buildingName));
        GameManager.Instance.CursorState = CursorState.Building;
    }

    void MoveBuildingToMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        var raycastHit = ECSRaycast.Raycast(ray.origin, ray.direction * 100, collisionLayerMask);
        currentObject.transform.position = raycastHit.Position + new float3(0, currentObject.transform.localScale.y / 2, 0);
    }

    void PlaceCurrentObject()
    {
        EntityManager entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        currentObject.AddComponent<ConvertToEntity>();

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
