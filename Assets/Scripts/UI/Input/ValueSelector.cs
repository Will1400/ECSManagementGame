using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Events;

public class ValueSelector : MonoBehaviour
{
    public UnityEvent<int> OnValueChanged;

    public string[] Values;
    public int CurrentIndex;

    [SerializeField]
    private TextMeshProUGUI valueText;


    private void Awake()
    {
        if (OnValueChanged == null)
            OnValueChanged = new UnityEvent<int>();
    }

    private void Start()
    {
        valueText.text = Values[CurrentIndex];
    }

    public void UpdateDisplayedText()
    {
        valueText.text = Values[CurrentIndex % Values.Length];
    }

    public void SelectNext()
    {
        CurrentIndex++;
        CurrentIndex %= Values.Length;

        valueText.text = Values[CurrentIndex];
        OnValueChanged.Invoke(CurrentIndex);
    }

    public void SelectPrevious()
    {
        CurrentIndex--;

        if (CurrentIndex < 0)
            CurrentIndex = Values.Length - 1;

        valueText.text = Values[CurrentIndex % Values.Length];
        OnValueChanged.Invoke(CurrentIndex);
    }
}
