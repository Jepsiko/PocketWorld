using System;
using UnityEngine;

[Serializable]
public class Biome {

    public string biomeName;

    public bool enabled;

    public Vector2 temperatureRange;
    public Vector2 humidityRange;

    public Color biomeColor;
    public Gradient waterGradient;
    public Gradient beachGradient;
    public Gradient landGradient;
    public Gradient peakGradient;

    public float treeProbability;
    public float treeRange;
    public GameObject[] trees;

    [HideInInspector]
    public Settings settings;

    public bool IsValid(float temperature, float humidity) {
        return temperatureRange.x <= temperature && temperature <= temperatureRange.y &&
                humidityRange.x <= humidity && humidity <= humidityRange.y;
    }
    
    public bool IsAlmostValid(float temperature, float humidity) {
        return temperatureRange.x - settings.biomeMargin <= temperature && temperature <= temperatureRange.y + settings.biomeMargin &&
                humidityRange.x - settings.biomeMargin <= humidity && humidity <= humidityRange.y + settings.biomeMargin;
    }

    public Color GetColor(float height, float slope) {
        float normalizedHeight;
        if (height < settings.waterLevel) {
            normalizedHeight = (height + 1) / (settings.waterLevel + 1);
            return waterGradient.Evaluate(normalizedHeight);
        }
        else {
            normalizedHeight = (height - settings.waterLevel) / (1 - settings.waterLevel);
            if (normalizedHeight < settings.beachLevel)
                return beachGradient.Evaluate(normalizedHeight / settings.beachLevel);
            else if (normalizedHeight > settings.peakLevel)
                return peakGradient.Evaluate((normalizedHeight - settings.peakLevel) / (1 - settings.peakLevel));
            else
                return landGradient.Evaluate(slope * settings.slopeMultiplier);
        }
    }
}
