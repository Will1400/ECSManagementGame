using UnityEngine;
using System.Collections;

public abstract class GraphicsControllerBase : MonoBehaviour
{
    protected int currentLevel;

    protected virtual void Start()
    {
        GraphicConfigManager.Instance.OnConfigLoaded += ConfigLoaded;
        GraphicConfigManager.Instance.OnConfigChanged += ConfigChanged;
    }

    public virtual void PresetChanged(int level)
    {

    }

    public virtual void ConfigLoaded()
    {

    }

    public virtual void ConfigChanged()
    {

    }

    private void OnDestroy()
    {
        GraphicConfigManager.Instance.OnConfigLoaded -= ConfigLoaded;
        GraphicConfigManager.Instance.OnConfigChanged -= ConfigChanged;
    }
}
