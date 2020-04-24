using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

[ExecuteAlways]
public class DayNightLightingManager : MonoBehaviour
{
    [SerializeField]
    private Light directionalLight;
    [SerializeField]
    private DayNightLightingPreset lightingPreset;
    [SerializeField]
    private Volume volume;

    [SerializeField, Range(0, 24)]
    private float timeOfDay;

    private void Update()
    {
        if (lightingPreset == null)
            return;

        if (Application.isPlaying)
        {
            timeOfDay += Time.deltaTime / 3;
            timeOfDay %= 24;
            UpdateLighting(timeOfDay / 24);
        }
    }

    void UpdateLighting(float timePercent)
    {
        volume.profile.TryGet(out Fog fog);
        fog.albedo.value = lightingPreset.FogColor.Evaluate(timePercent);
        fog.albedo.overrideState = true;

        directionalLight.color = lightingPreset.DirectionalColor.Evaluate(timePercent);
        directionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360) - 90, 37, 0));
    }
}
