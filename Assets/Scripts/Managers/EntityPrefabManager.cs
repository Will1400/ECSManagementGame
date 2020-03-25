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
using System.Runtime.Serialization;

public class EntityPrefabManager : MonoBehaviour
{
    public IComponentData test;

    public static EntityPrefabManager Instance;

    public Dictionary<string, Entity> Prefabs = new Dictionary<string, Entity>();

    EntityManager EntityManager;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
    }

    private void Start()
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        using (BlobAssetStore assetStore = new BlobAssetStore())
        {
            var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, assetStore);
            GameObject[] gameObjectPrefabs = Resources.LoadAll<GameObject>("Prefabs/");

            foreach (var item in gameObjectPrefabs)
            {
                var entity = GameObjectConversionUtility.ConvertGameObjectHierarchy(item, settings);
                EntityManager.AddComponent<Disabled>(entity);
                Prefabs.Add(item.name, entity);
            }
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
}
