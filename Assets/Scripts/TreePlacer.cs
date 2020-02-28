using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreePlacer : MonoBehaviour
{
    [SerializeField]
    List<GameObject> TreePrefabs;
    [SerializeField]
    GameObject ground;

    [SerializeField]
    float2 mapSize;
    [SerializeField]
    Texture2D texture;
    [SerializeField]
    float threshold = .4f;
    [SerializeField]
    int randomMax = 1;

    Queue<GameObject> SpawnedTrees;
    private void Start()
    {
        if (mapSize.Equals(float2.zero))
        {
            Mesh planeMesh = ground.GetComponent<MeshFilter>().mesh;
            Bounds bounds = planeMesh.bounds;
            float boundsX = ground.transform.localScale.x * bounds.size.x;
            float boundsZ = ground.transform.localScale.z * bounds.size.z;
            mapSize = new float2(boundsX, boundsZ);
        }
        SpawnedTrees = new Queue<GameObject>();
        texture = new Texture2D((int)mapSize.x, (int)mapSize.y);
        PlaceTrees();
    }

    void PlaceTrees()
    {

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                var point = Mathf.PerlinNoise(x / mapSize.x, y / mapSize.y);
                point = point + 0.01f;

                texture.SetPixel(x,y, new Color(point, point, point));
                if (point >= threshold && Random.Range(0, randomMax) == 0)
                {
                    var tree = TreePrefabs[Random.Range(0, TreePrefabs.Count - 1)];
                    var spawnedTree = Instantiate(tree, new Vector3(x, 0, y), tree.transform.rotation);
                    spawnedTree.transform.eulerAngles += (new Vector3(0, Random.Range(0, 360)));

                    if (IsTreePlacementValid(spawnedTree.GetComponent<Collider>()))
                    {
                        SpawnedTrees.Enqueue(spawnedTree);
                    }
                    else
                    {
                        Destroy(spawnedTree);
                    }
                }
            }
        }
    }

    bool IsTreePlacementValid(Collider spawnedTreeCollider)
    {
        foreach (Collider tree in SpawnedTrees.Select(x => x.GetComponent<Collider>()))
        {
            if (spawnedTreeCollider.bounds.Intersects(tree.bounds))
            {
                return false;
            }
        }
        return true;
    }

    void RemoveAllTrees()
    {
        while (SpawnedTrees.Count > 0)
        {
            Destroy(SpawnedTrees.Dequeue());
        }
    }

    public void Replant()
    {
        RemoveAllTrees();
        PlaceTrees();
    }
}
