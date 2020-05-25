using UnityEngine;
using System.Collections;
using TMPro;
using System.Linq;

public class ResolutionController : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown dropdown;

    Resolution[] resolutions;

    private void Start()
    {
        resolutions = Screen.resolutions;
        var currentResolution = Screen.currentResolution;

        dropdown.ClearOptions();
        dropdown.AddOptions(resolutions.Select(x => x.ToString()).ToList());

        for (int i = 0; i < resolutions.Length; i++)
        {
            if (resolutions[i].height == currentResolution.height && resolutions[i].width == currentResolution.width)
            {
                dropdown.SetValueWithoutNotify(i);
                dropdown.RefreshShownValue();
                break;
            }
        }
    }

    public void SetResolution(int valueIndex)
    {
        var resolution = resolutions[valueIndex];

        Screen.SetResolution(resolution.width, resolution.height, (FullScreenMode)GraphicConfigManager.Instance.GraphicConfiguration["Display"]["FullScreenMode"].IntValue);
    }
}
