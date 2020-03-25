using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class BuildUIManager : MonoBehaviour
{
    public static BuildUIManager Instance;

    // Prefabs loaded
    Dictionary<string, List<GameObject>> PrefabContexts;

    // UI elements
    Dictionary<string, List<GameObject>> buildItemContexts;

    [SerializeField]
    Sprite defaultSprite;
    [SerializeField]
    GameObject buildPanel;
    [SerializeField]
    TabGroup BuildTabGroup;
    [SerializeField]
    Transform contentHolder;

    [SerializeField]
    GameObject buildItemPrefab;

    private void Awake()
    {
        if (Instance is null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        ClosePanel();
        foreach (Transform item in contentHolder)
        {
            item.gameObject.SetActive(false);
        }

        PrefabContexts = new Dictionary<string, List<GameObject>>();
        buildItemContexts = new Dictionary<string, List<GameObject>>();

        PrefabContexts.Add("Houses", Resources.LoadAll<GameObject>("Prefabs/AutoLoadedPrefabs/Buildings/Houses/").ToList());
        PrefabContexts.Add("ResourceGatherers", Resources.LoadAll<GameObject>("Prefabs/AutoLoadedPrefabs/Buildings/ResourceGatherers/").ToList());
        PrefabContexts.Add("All", Resources.LoadAll<GameObject>("Prefabs/AutoLoadedPrefabs/").ToList());

        List<GameObject>[] values = PrefabContexts.Values.ToArray();
        string[] keys = PrefabContexts.Keys.ToArray();
        for (int i = 0; i < keys.Length; i++)
        {
            List<GameObject> objectList = values[i];
            string key = keys[i];

            var setupBuildOptions = new List<GameObject>();
            for (int j = 0; j < objectList.Count; j++)
            {
                GameObject instantiatedBuildOption = Instantiate(buildItemPrefab);
                instantiatedBuildOption.transform.SetParent(contentHolder, false);

                var sprite = Resources.Load<Sprite>($"Sprites/BuildingPreviews/{objectList[j].name}");
                instantiatedBuildOption.GetComponent<BuildItem>().Initialize(objectList[j].name, sprite ? sprite : defaultSprite);

                setupBuildOptions.Add(instantiatedBuildOption);
            }

            buildItemContexts.Add(key, setupBuildOptions);
        }

        SwitchContextTo("Houses");
    }

    void Update()
    {
        if (Input.GetButtonDown("Build"))
            TogglePanel();
        if (Input.GetKeyDown(KeyCode.Escape))
            ClosePanel();

        if (buildPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Q))
                BuildTabGroup.SelectLeft();
            if (Input.GetKeyDown(KeyCode.E))
                BuildTabGroup.SelectRight();
        }
    }

    public void SwitchContextTo(string name)
    {
        if (buildItemContexts == null)
            return;

        if (buildItemContexts.TryGetValue(name, out List<GameObject> objectsToEnable))
        {
            foreach (List<GameObject> item in buildItemContexts.Values)
            {
                for (int i = 0; i < item.Count; i++)
                {
                    item[i].SetActive(false);
                }
            }

            for (int j = 0; j < objectsToEnable.Count; j++)
            {
                objectsToEnable[j].SetActive(true);
            }
        }
    }

    public void OpenPanel()
    {
        buildPanel.SetActive(true);
    }

    public void ClosePanel()
    {
        buildPanel.SetActive(false);
    }

    public void TogglePanel()
    {
        buildPanel.SetActive(!buildPanel.activeSelf);
    }
}
