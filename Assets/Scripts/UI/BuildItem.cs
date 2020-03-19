using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class BuildItem : MonoBehaviour
{
    public TextMeshProUGUI NameText;
    public Image PreviewImage;

    public void Initialize(string buildingName, Sprite sprite)
    {
        NameText.text = buildingName;
        PreviewImage.sprite = sprite;
    }
}
