using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NoiseSetting {

    [Range(0, 1)]
    public float roughness = 0.1f;
    [Range(0, 1)]
    public float strength = 1;
    [Range(1, 4)]
    public int layersCount = 1;
    [Range(1, 4)]
    public float lacunarity = 2;
    [Range(0, 1)]
    public float persistance = 0.25f;
    public bool useFirstLayerAsMask;

    public Vector3Int center;

    public float Evaluate(Noise noise, Vector3Int coord) {
        float value = noise.Evaluate((Vector3) (coord + center) * (roughness / 10)) * strength;
        for (int k = 1; k < layersCount; k++) {
            value += (noise.Evaluate((Vector3) (coord + center) * (roughness / 10) * Mathf.Pow(lacunarity, k)) * strength * Mathf.Pow(persistance, k));
        }
        return value;
    }
}
