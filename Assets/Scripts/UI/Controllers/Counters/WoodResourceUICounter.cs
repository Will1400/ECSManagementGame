using UnityEngine;
using System.Collections;
using TMPro;

public class WoodResourceUICounter : MonoBehaviour
{
    public TextMeshProUGUI CountText;

    private void Start()
    {
        if (CountText == null && TryGetComponent(out TextMeshProUGUI text))
            CountText = text;

        SetCount(0);
    }

    private void OnEnable()
    {
        UIStatUpdatingSystem.Instance.WoodResourceCountChanged += SetCount;
    }

    public void SetCount(int count)
    {
        CountText.text = count.ToString();
    }

    private void OnDisable()
    {
        UIStatUpdatingSystem.Instance.WoodResourceCountChanged -= SetCount;
    }
}
