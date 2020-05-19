using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class QualityPresetController : MonoBehaviour
{
    public UnityEvent<int> OnPresetChanged;

    [SerializeField]
    ValueSelector valueSelector;

    private void Start()
    {
        OnPresetChanged = new UnityEvent<int>();
        valueSelector.Values = QualitySettings.names;
    }

    public void SetQuality(int level)
    {
        QualitySettings.SetQualityLevel(level, true);
        OnPresetChanged.Invoke(level);
    }
}
