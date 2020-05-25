using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.HighDefinition;
using WHK.Config;
using Newtonsoft.Json.Schema;
using System;

public class AntiAliasingSettingController : GraphicsControllerBase
{
    [SerializeField]
    ValueSelector valueSelector;

    public override void ConfigLoaded()
    {
        ApplyChanges();
    }

    public override void ConfigChanged()
    {
        ApplyChanges();
    }

    public override void PresetChanged(int level)
    {
    }

    public void ChangeLevel(int level)
    {
        GraphicConfigManager.Instance.UpdateConfigSetting("AntiAliasing", "AntialiasingMode", level);
    }

    public void ApplyChanges()
    {
        var cameraData = Camera.main.GetComponent<HDAdditionalCameraData>();
        int mode = GraphicConfigManager.Instance.GraphicConfiguration["AntiAliasing"]["AntiAliasingMode"].IntValue;
        cameraData.antialiasing = (HDAdditionalCameraData.AntialiasingMode)mode;

        valueSelector.CurrentIndex = mode;
        valueSelector.UpdateDisplayedText();
    }

}
