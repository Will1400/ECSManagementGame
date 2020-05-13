using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI NameText;
    public Image PreviewImage;
    public Button BuildButton;

    public void Initialize(string buildingName, Sprite sprite)
    {
        NameText.text = buildingName;
        PreviewImage.sprite = sprite;

        BuildButton.onClick.AddListener(StartBuildPlacement);
    }

    void StartBuildPlacement()
    {
        PlacementSystem.Instance.Spawn(NameText.text, true);
        BuildUIManager.Instance.ClosePanel();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BuildItemHoverPreviewUIManager.Instance.HoverStart(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BuildItemHoverPreviewUIManager.Instance.HoverEnd();
    }
}
