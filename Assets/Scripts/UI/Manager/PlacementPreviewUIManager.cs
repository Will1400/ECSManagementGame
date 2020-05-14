using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class PlacementPreviewUIManager : MonoBehaviour
{
    public static PlacementPreviewUIManager Instance;

    [SerializeField]
    private GameObject panelObject;
    [SerializeField]
    private GameObject hotbarObject;
    [SerializeField]
    private bool hideWindows;
    [SerializeField]
    private GameObject windowHolderObject;

    [SerializeField]
    private GameObject contentHolder;
    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private TextMeshProUGUI descriptionText;
    [SerializeField]
    private Image previewImage;

    string prefabName;
    EntityManager EntityManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;

        panelObject.SetActive(false);
    }

    private void Start()
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    public void ShowPreview(string prefabName)
    {
        this.prefabName = prefabName;

        if (hideWindows)
            windowHolderObject.SetActive(false);

        hotbarObject.SetActive(false);

        SetupPanel();
    }

    public void HidePreview()
    {
        if (hideWindows)
            windowHolderObject.SetActive(true);

        hotbarObject.SetActive(true);
        panelObject.SetActive(false);
    }

    void SetupPanel()
    {
        var previewedEntity = EntityPrefabManager.Instance.GetEntityPrefab(prefabName);

        var displayData = EntityManager.GetComponentData<DisplayData>(previewedEntity);
        titleText.text = displayData.Name.ToString();
        descriptionText.text = "";

        var sprite = Resources.Load<Sprite>("Sprites/BuildingPreviews/" + prefabName);
        if (sprite != null)
            previewImage.sprite = sprite;

        if (EntityManager.HasComponent<HasNoResourceCost>(previewedEntity))
        {
            contentHolder.SetActive(false);
        }
        else
        {
            contentHolder.SetActive(true);
            var costBuffer = EntityManager.GetBuffer<ResourceCostElement>(previewedEntity);

            foreach (Transform item in contentHolder.transform)
            {
                item.gameObject.SetActive(false);
            }

            foreach (var item in costBuffer)
            {
                var amountObject = contentHolder.transform.Find(item.Value.ResourceType.ToString() + "/Amount");

                amountObject.parent.gameObject.SetActive(true);

                amountObject.GetComponent<TextMeshProUGUI>().text = item.Value.Amount.ToString();

            }
        }

        panelObject.SetActive(true);
    }
}
