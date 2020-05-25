using UnityEngine;
using System.Collections;
using WHK.Config;
using System.IO;
using SharpConfig;
using System;

public class GraphicConfigManager : MonoBehaviour
{
    public static GraphicConfigManager Instance;

    public event Action OnConfigLoaded;
    public event Action OnConfigChanged;
    public event Action OnConfigSaved;

    public Configuration GraphicConfiguration;

    string configFile = "/Graphics.config";

    float configVersion = 0.2f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        if (File.Exists(Application.persistentDataPath + configFile))
        {
            LoadConfig();
        }
        else
        {
            CreateNewConfig();
        }
    }

    public void CreateNewConfig()
    {
        var config = new Configuration();
        config["Version"]["ConfigVersion"].FloatValue = configVersion;

        config["Display"]["ResolutionWidth"].IntValue = 1920;
        config["Display"]["ResolutionHeight"].IntValue = 1080;
        config["Display"]["FullScreenMode"].IntValue = 3;

        config.Add(Section.FromObject("AntiAliasing", new AntiAliasingInfo { AntiAliasingQuality = AntiAliasingQuality.Medium, AntialiasingMode = 1 }));
        SaveConfig();
    }

    public void UpdateConfigSetting(string section, string name, object value)
    {
        GraphicConfiguration[section][name].SetValue(value);
        ApplyChanges();
    }

    public void ApplyChanges()
    {
        OnConfigChanged?.Invoke();
        SaveConfig();
    }

    public void LoadConfig()
    {
        GraphicConfiguration = Configuration.LoadFromFile(Application.persistentDataPath + configFile);
        OnConfigLoaded?.Invoke();
    }

    public void SaveConfig()
    {
        GraphicConfiguration.SaveToFile(Application.persistentDataPath + configFile);
        OnConfigSaved?.Invoke();
    }
}
