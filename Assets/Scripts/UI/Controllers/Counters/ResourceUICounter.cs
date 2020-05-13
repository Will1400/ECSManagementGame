using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResourceUICounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI countText;
    [SerializeField]
    private ResourceType selectedResourceType;

    private void Start()
    {
        if (countText == null && TryGetComponent(out TextMeshProUGUI text))
            countText = text;

        SetCount(selectedResourceType, 0);
    }

    private void OnEnable()
    {
        UIStatUpdatingSystem.Instance.ResourceCountChanged += SetCount;
    }

    public void SetCount(ResourceType resourceType, int count)
    {
        if (resourceType == this.selectedResourceType)
            countText.text = count.ToString();
    }

    private void OnDisable()
    {
        UIStatUpdatingSystem.Instance.ResourceCountChanged -= SetCount;
    }
}
