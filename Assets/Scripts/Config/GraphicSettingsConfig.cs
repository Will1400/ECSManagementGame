using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.HighDefinition;

namespace WHK.Config
{
    public class GraphicSettingsConfig
    {
        // Display Settings
        public int ResolutionWidth = 1920;
        public int ResolutionHeight = 1080;

        public FullScreenMode FullScreenMode = FullScreenMode.FullScreenWindow;

        // Graphic Settings
        public AntiAliasingInfo AntiAliasingInfo = new AntiAliasingInfo { AntiAliasingQuality = AntiAliasingQuality.Medium};
    }
}
