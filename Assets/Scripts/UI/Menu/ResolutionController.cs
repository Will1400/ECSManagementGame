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

        dropdown.ClearOptions();
        dropdown.AddOptions(resolutions.Select(x => x.ToString()).ToList());
    }

    public void SetResolution(int valueIndex)
    {
        var resolution = resolutions[valueIndex];

        Screen.SetResolution(resolution.width, resolution.height, FullScreenMode.FullScreenWindow);
        Debug.Log(Screen.currentResolution);
        
    }
}
