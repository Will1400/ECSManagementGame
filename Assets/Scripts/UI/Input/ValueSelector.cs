using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class ValueSelector : MonoBehaviour
{
    public UnityEvent<int> OnValueChanged;

    public string[] Values;

    [SerializeField]
    private TextMeshProUGUI valueText;

    int currentIndex;

    private void Start()
    {
        OnValueChanged = new UnityEvent<int>();
    }

    public void SelectNext()
    {
        currentIndex++;

        valueText.text = Values[currentIndex % Values.Length];
    }

    public void SelectPrevious()
    {
        currentIndex--;

        if (currentIndex < 0)
            currentIndex = Values.Length - 1;

        valueText.text = Values[currentIndex % Values.Length];
    }
}
