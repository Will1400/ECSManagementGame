using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine.Rendering;
using System.Linq;
using Unity.Mathematics;
using System;
using Random = UnityEngine.Random;

public class EntityPrefabManager : MonoBehaviour
{
    public IComponentData test;

    public static EntityPrefabManager Instance;

    public Dictionary<string, Entity> Prefabs = new Dictionary<string, Entity>();

    EntityManager EntityManager;

    BlobAssetStore assetStore;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
    }

    private void Start()
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        assetStore = new BlobAssetStore();
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, assetStore);
        GameObject[] gameObjectPrefabs = Resources.LoadAll<GameObject>("Prefabs/");

        foreach (var item in gameObjectPrefabs)
        {
            var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(item, settings);

            EntityManager.AddComponent<Disabled>(entity);

            EntityManager.AddComponent<DisplayData>(entity);
            EntityManager.SetComponentData(entity, new DisplayData { Name = item.name });

            Prefabs.Add(item.name, entity);
        }
    }

    public Entity SpawnEntityPrefab(string name)
    {
        if (Prefabs.TryGetValue(name, out Entity prefab))
        {
            Entity entity = EntityManager.Instantiate(prefab);
            EntityManager.RemoveComponent<Disabled>(entity);
            return entity;
        }

        return Entity.Null;
    }

    public Entity SpawnEntityPrefab(EntityCommandBuffer commandBuffer, string name)
    {
        if (Prefabs.TryGetValue(name, out Entity prefab))
        {
            Entity entity = commandBuffer.Instantiate(prefab);
            commandBuffer.RemoveComponent<Disabled>(entity);
            return entity;
        }

        return Entity.Null;
    }

    public Entity GetEntityPrefab(string name)
    {
        if (Prefabs.TryGetValue(name, out Entity prefab))
        {
            return prefab;
        }

        return Entity.Null;
    }

    public Entity SpawnCitizenPrefab(EntityCommandBuffer commandBuffer)
    {
        if (Prefabs.TryGetValue("Citizen", out Entity prefab))
        {
            Entity entity = commandBuffer.Instantiate(prefab);
            commandBuffer.RemoveComponent<Disabled>(entity);

            var gender = (Gender)Random.Range(1, 3);
            commandBuffer.SetComponent(entity, new Citizen
            {
                CitizenPersonalData = new CitizenPersonalInfo
                {
                    Name = NameHelper.GetName(gender),
                    Age = Random.Range(10, 40),
                    Gender = gender
                }
            });
            return entity;
        }
        return Entity.Null;
    }

    public Entity SpawnCitizenPrefab()
    {
        if (Prefabs.TryGetValue("Citizen", out Entity prefab))
        {
            Entity entity = EntityManager.Instantiate(prefab);
            EntityManager.RemoveComponent<Disabled>(entity);

            var gender = (Gender)Random.Range(1, 3);
            EntityManager.AddComponentData(entity, new Citizen
            {
                CitizenPersonalData = new CitizenPersonalInfo
                {
                    Name = NameHelper.GetName(gender),
                    Age = Random.Range(10, 40),
                    Gender = gender
                }
            });
            return entity;
        }
        return Entity.Null;
    }

    public Entity SpawnConstructionEntityPrefab(string name)
    {
        if (Prefabs.TryGetValue(name, out Entity prefab))
        {
            Entity entity = EntityManager.CreateEntity(ArcheTypeManager.Instance.GetArcheType(PredifinedArchetype.ConstructionSite));

            if (EntityManager.HasComponent<HasNoResourceCost>(prefab))
            {
                EntityManager.AddComponent<HasNoResourceCost>(entity);
            }
            else
            {
                EntityManager.AddBuffer<ResourceCostElement>(entity);

                DynamicBuffer<ResourceCostElement> buildCostBuffer = EntityManager.GetBuffer<ResourceCostElement>(prefab);
                if (buildCostBuffer.IsCreated && buildCostBuffer.Length > 0)
                {
                    DynamicBuffer<ResourceCostElement> resourceCostBuffer = EntityManager.AddBuffer<ResourceCostElement>(entity);

                    resourceCostBuffer.CopyFrom(EntityManager.GetBuffer<ResourceCostElement>(prefab));
                }
            }

            EntityManager.AddComponentData(entity, new ResourceStorageData { MaxCapacity = -1 });

            return entity;
        }

        return Entity.Null;
    }

    public Entity SpawnBeingPlacedEntityPrefab(string name)
    {
        if (Prefabs.TryGetValue(name, out Entity prefab))
        {
            Entity entity = SpawnVisualOnlyPrefab(name);

            if (EntityManager.HasComponent<HasNoResourceCost>(prefab))
            {
                EntityManager.AddComponent<HasNoResourceCost>(entity);
            }
            else
            {
                EntityManager.AddBuffer<ResourceCostElement>(entity);

                DynamicBuffer<ResourceCostElement> buildCostBuffer = EntityManager.GetBuffer<ResourceCostElement>(prefab);
                if (buildCostBuffer.IsCreated && buildCostBuffer.Length > 0)
                {
                    DynamicBuffer<ResourceCostElement> resourceCostBuffer = EntityManager.AddBuffer<ResourceCostElement>(entity);

                    resourceCostBuffer.CopyFrom(EntityManager.GetBuffer<ResourceCostElement>(prefab));
                }
            }

            EntityManager.AddComponent<BeingPlacedTag>(entity);

            // Add grid occupation
            EntityManager.AddComponent<GridOccupation>(entity);
            int4 positions = GridHelper.CalculateGridOccupationFromBounds(EntityManager.GetComponentData<RenderBounds>(entity).Value);
            EntityManager.AddComponentData(entity, new GridOccupation { Start = new int2(positions.x, positions.y), End = new int2(positions.z, positions.w) });

            return entity;
        }

        return Entity.Null;
    }

    public Entity SpawnVisualOnlyPrefab(string name)
    {
        if (Prefabs.TryGetValue(name, out Entity prefab))
        {
            var entity = EntityManager.CreateEntity(
                ComponentType.ReadOnly<Translation>(),
                ComponentType.ReadOnly<Rotation>(),
                ComponentType.ReadOnly<Scale>(),
                ComponentType.ReadOnly<RenderMesh>(),
                ComponentType.ReadOnly<RenderBounds>(),
                ComponentType.ReadOnly<WorldRenderBounds>(),
                ComponentType.ReadOnly<LocalToWorld>());

            if (EntityManager.HasComponent<NonUniformScale>(prefab))
                EntityManager.AddComponentData(entity, EntityManager.GetComponentData<NonUniformScale>(prefab));
            else if (EntityManager.HasComponent<Scale>(prefab))
                EntityManager.AddComponentData(entity, EntityManager.GetComponentData<Scale>(prefab));
            else
                EntityManager.AddComponentData(entity, new Scale { Value = 1 });

            EntityManager.AddComponentData(entity, EntityManager.GetComponentData<Translation>(prefab));
            EntityManager.AddComponentData(entity, EntityManager.GetComponentData<Rotation>(prefab));
            EntityManager.AddComponentData(entity, EntityManager.GetComponentData<RenderBounds>(prefab));
            EntityManager.AddSharedComponentData(entity, EntityManager.GetSharedComponentData<RenderMesh>(prefab));
            return entity;
        }

        return Entity.Null;
    }
}
