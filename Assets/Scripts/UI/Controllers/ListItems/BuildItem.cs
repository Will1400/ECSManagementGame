using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class BuildItem : MonoBehaviour
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
}
