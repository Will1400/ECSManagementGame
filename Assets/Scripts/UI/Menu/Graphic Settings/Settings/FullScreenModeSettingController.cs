using UnityEngine;
using System.Collections;
using TMPro;
using System;
using System.Linq;

public class FullScreenModeSettingController : GraphicsControllerBase
{
    [SerializeField]
    private TMP_Dropdown dropdown;

    protected override void Start()
    {
        base.Start();
        dropdown.ClearOptions();
        dropdown.AddOptions(Enum.GetNames(typeof(FullScreenMode)).ToList());

        ApplyChanges();
    }

    public override void ConfigLoaded()
    {
        ApplyChanges();
    }

    public override void ConfigChanged()
    {
        ApplyChanges();
    }

    public void ChangeLevel(int level)
    {
        GraphicConfigManager.Instance.UpdateConfigSetting("Display", "FullScreenMode", level);
    }

    public void ApplyChanges()
    {
        int mode = GraphicConfigManager.Instance.GraphicConfiguration["Display"]["FullScreenMode"].IntValue;
        Screen.fullScreenMode = (FullScreenMode)mode;
        dropdown.value = mode;
        dropdown.RefreshShownValue();
    }
}
