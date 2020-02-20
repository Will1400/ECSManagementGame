using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class PrefabsManager : MonoBehaviour
{
    public static PrefabsManager Instance;

    public List<GameObject> Prefabs;
    public NativeList<Entity> EntityPrefabs;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        EntityPrefabs = new NativeList<Entity>(Allocator.Persistent);
    }

    void Start()
    {
        foreach (var item in Prefabs)
        {
            using (BlobAssetStore blobAssetStore = new BlobAssetStore())
            {
                EntityPrefabs.Add(GameObjectConversionUtility.ConvertGameObjectHierarchy(item, GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, blobAssetStore)));
                Debug.Log(EntityPrefabs[0]);
            }
        }
    }
    private void OnDestroy()
    {
        EntityPrefabs.Dispose();
    }
}
