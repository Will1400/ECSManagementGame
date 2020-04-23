using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class InventoryItem : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;
    [SerializeField]
    private TextMeshProUGUI valueText;

    public void Initialize(string name, int amount)
    {
        nameText.text = name;
        valueText.text = amount.ToString();
    }
}
