using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Entities;
using UnityEngine;

public class CitizenUICounter : MonoBehaviour
{
    public TextMeshProUGUI CountText;

    private void Start()
    {
        SetCount(0);
    }

    private void OnEnable()
    {
        UIStatUpdatingSystem.Instance.CitizenCountChanged += SetCount;
    }

    public void SetCount(int count)
    {
        CountText.text = count.ToString();
    }

    private void OnDisable()
    {
        UIStatUpdatingSystem.Instance.CitizenCountChanged -= SetCount;

    }
}
