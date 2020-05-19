using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;
using Unity.Entities;
using Unity.Properties;
using UnityEngine;
using UnityEngine.UI;

public class BuildItemHoverPreviewUIManager : MonoBehaviour
{
    public static BuildItemHoverPreviewUIManager Instance;

    [SerializeField]
    private Canvas canvas;
    [SerializeField]
    private RectTransform canvasRectTransform;
    [SerializeField]
    private GameObject panelObject;
    [SerializeField]
    private GameObject contentHolder;
    [SerializeField]
    private TextMeshProUGUI titleText;
    [SerializeField]
    private TextMeshProUGUI descriptionText;
    [SerializeField]
    private Image previewImage;

    [SerializeField]
    private float instantHoverTime = .5f;
    [SerializeField]
    private float timeToHover = .5f;

    EntityManager EntityManager;

    BuildItem buildItem;

    float hoverTimeRemaining;
    float instantHoverTimeRemaining;
    bool isHovering;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        EntityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    private void Update()
    {
        if (panelObject.activeSelf && (buildItem == null || !buildItem.gameObject.activeSelf || !BuildUIManager.Instance.IsActive))
            HoverEnd();


        if (isHovering)
        {
            if (instantHoverTimeRemaining > 0)
            {
                hoverTimeRemaining = 0;
            }

            hoverTimeRemaining -= Time.deltaTime;

            if (hoverTimeRemaining <= 0 && !panelObject.activeSelf)
            {
                SetupPanel();
            }
        }
        else
        {
            if (instantHoverTimeRemaining >= 0)
                instantHoverTimeRemaining -= Time.deltaTime;
        }
    }

    public void HoverStart(BuildItem buildItem)
    {
        isHovering = true;

        this.buildItem = buildItem;
    }

    public void HoverEnd()
    {
        isHovering = false;
        hoverTimeRemaining = timeToHover;

        instantHoverTimeRemaining = instantHoverTime;

        panelObject.SetActive(false);
        buildItem = null;
    }

    void SetupPanel()
    {
        var previewedEntity = EntityPrefabManager.Instance.GetEntityPrefab(buildItem.NameText.text);
        var panelRectTransform = panelObject.gameObject.GetComponent<RectTransform>();

        var buildRectTransform = buildItem.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTransform, buildRectTransform.anchoredPosition, canvas.worldCamera, out Vector2 point);

        point.y = panelRectTransform.anchoredPosition.y;
        point.x += panelRectTransform.rect.xMax - panelRectTransform.rect.xMin;
        panelRectTransform.anchoredPosition = point;

        var displayData = EntityManager.GetComponentData<DisplayData>(previewedEntity);
        titleText.text = displayData.Name.ToString();
        descriptionText.text = "";

        previewImage.sprite = buildItem.PreviewImage.sprite;

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
