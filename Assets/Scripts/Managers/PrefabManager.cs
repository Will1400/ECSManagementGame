using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabManager : MonoBehaviour
{
    public static PrefabManager Instance;

    [SerializeField]
    private List<GameObject> buildingPrefabs;
    [SerializeField]
    private List<GameObject> unitPrefabs;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public GameObject GetBuilding(string buildingName)
    {
        return buildingPrefabs.First(x => x.name == buildingName);
    }

    public GameObject GetBuilding(int buildingIndex)
    {
        return buildingPrefabs[buildingIndex];
    }

    public int GetIndexOfBuildingName(string buildingName)
    {
        return buildingPrefabs.Select(x => x.name).ToList().IndexOf(buildingName);
    }

    public GameObject GetUnit(string unitName)
    {
        return unitPrefabs.First(x => x.name == unitName);
    }

    public GameObject GetUnit(int unitIndex)
    {
        return unitPrefabs[unitIndex];
    }

    public int GetIndexOfUnitName(string unitName)
    {
        return unitPrefabs.Select(x => x.name).ToList().IndexOf(unitName);
    }

    public GameObject GetPrefabByName(string name)
    {
        if (name.Contains("(Clone)"))
        {
            name = name.Replace("(Clone)", string.Empty);
        }
        return buildingPrefabs.Concat(unitPrefabs).FirstOrDefault(x => x.name == name);
    }
}
