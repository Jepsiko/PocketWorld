using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Settings {

    public int chunkSize;
    public int initialChunkLoadDistance;
    public int chunkLoadDistance;
    public int chunkUnloadDistance;
    public float tileSize;
    public float waterLevel;
    public float beachLevel;
    public float peakLevel;
    public float heightAmplifier;
    public float slopeMultiplier;
    public AnimationCurve continentalnessSpline;
    public AnimationCurve erosionSpline;
    public AnimationCurve peaksAndValleysSpline;
    public int biomeMergeDistance;
    public float biomeMargin;
    [Range(0, 1)]
    public float treeDensity;
    public float treeMargin;
}
