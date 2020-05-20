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
        GraphicConfigManager.Instance.UpdateConfigSetting("AntiAliasing", "AntialiasingMode", (HDAdditionalCameraData.AntialiasingMode)level);
        Debug.Log("Updating AA to: " + (HDAdditionalCameraData.AntialiasingMode)level);
    }



    public void ApplyChanges()
    {
        var cameraData = Camera.main.GetComponent<HDAdditionalCameraData>();
        var antiAliasingInfo = GraphicConfigManager.Instance.GraphicConfiguration["AntiAliasing"].ToObject<AntiAliasingInfo>();
        cameraData.antialiasing = antiAliasingInfo.AntialiasingMode;

        valueSelector.CurrentIndex = (int)antiAliasingInfo.AntialiasingMode;
        valueSelector.UpdateDisplayedText();
    }

}
