using UnityEngine;
using System.Collections;
using System.Linq;

public class TextureQualitySettingController : GraphicsControllerBase
{
    [SerializeField]
    ValueSelector valueSelector;

    int[] textureSizes = new int[]
    {
        256,
        512,
        1024,
        2048
    };

    
    protected override void Start()
    {
        base.Start();

        valueSelector.Values = textureSizes.Select(x => x.ToString()).ToArray();
    }

    public override void PresetChanged(int level)
    {
    }

    public void SetQuality(int level)
    {
    }

    public override void ConfigLoaded()
    {
    }

    public override void ConfigChanged()
    {
    }
}
