using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Map : MonoBehaviour {

    public enum MapType {Normal, NoMerging, HeightMap, Continentalness, Erosion, PeaksAndValleys, Temperature, Humidity, Slope, Biomes};

    static Noise noiseContinentalness;
    static Noise noiseErosion;
    static Noise noisePeaksAndValleys;
    static Noise noiseTemperature;
    static Noise noiseHumidity;

    public PlayerController playerController;
    

    [Header("Debug")]
    public MapType mapType;

    public Settings settings;

    [Header("World settings")]
    public Biome[] biomes;

    [Header("Noises settings")]
    public NoiseSetting continentalness;
    public NoiseSetting erosion;
    public NoiseSetting peaksAndValleys;
    public NoiseSetting temperature;
    public NoiseSetting humidity;

    void Awake() {
        noiseContinentalness = new Noise(1);
        noiseErosion = new Noise(2);
        noisePeaksAndValleys = new Noise(3);
        noiseTemperature = new Noise(4);
        noiseHumidity = new Noise(5);

        foreach (Biome biome in biomes) {
            biome.settings = settings;
        }
    }

    public void LoadMap() {
        SaveSystem.Init();
        LoadSeed();
    }
    
    public void LoadSeed() {
        string seedString = SaveSystem.Load("seed");
        if (seedString != null) {
            SetSeed(int.Parse(seedString));
        }
        else {
            int seed = SetValidSeed();
            SaveSeed(seed);
        }
    }

    public void SaveSeed(int seed) {
        SaveSystem.Save(seed.ToString(), "seed");
    }

    public int SetValidSeed() {
        Vector3Int center = new Vector3Int();
        center.z = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        continentalness.center = center;
        erosion.center = center;
        peaksAndValleys.center = center;
        temperature.center = center;
        humidity.center = center;
        while (EvaluateHeight(Vector3Int.zero) < settings.waterLevel) {
            center.z = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            continentalness.center = center;
            erosion.center = center;
            peaksAndValleys.center = center;
            temperature.center = center;
            humidity.center = center;
        }
        return center.z;
    }

    public void SetSeed(int seed) {
        Vector3Int center = new Vector3Int();
        center.z = seed;
        continentalness.center = center;
        erosion.center = center;
        peaksAndValleys.center = center;
        temperature.center = center;
        humidity.center = center;
    }

    public float EvaluateContinentalness(Vector3Int coord) {
        return continentalness.Evaluate(noiseContinentalness, coord);
    }

    public float EvaluateErosion(Vector3Int coord) {
        return erosion.Evaluate(noiseErosion, coord);
    }

    public float EvaluatePeaksAndValleys(Vector3Int coord) {
        return peaksAndValleys.Evaluate(noisePeaksAndValleys, coord);
    }
    
    public float EvaluateHeight(Vector3Int coord) {
        float value = settings.continentalnessSpline.Evaluate(EvaluateContinentalness(coord));
        value *= settings.erosionSpline.Evaluate(EvaluateErosion(coord));
        value *= settings.peaksAndValleysSpline.Evaluate(EvaluatePeaksAndValleys(coord));
        return value * settings.heightAmplifier;
    }
    
    public float EvaluateTemperature(Vector3Int coord) {
        return temperature.Evaluate(noiseTemperature, coord);
    }

    public float EvaluateHumidity(Vector3Int coord) {
        return humidity.Evaluate(noiseHumidity, coord);
    }

    public float GetSlope(Vector3Int localCoord, Vector3Int chunkCoord) {
        if (GetComponent<ChunkLoader>().chunks.ContainsKey(chunkCoord))
            return GetComponent<ChunkLoader>().chunks[chunkCoord].GetSlope(localCoord);
        else {
            float slope = 0;
            float coordHeight = GetHeight(localCoord, chunkCoord);
            Vector3Int[] directions = {Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left};
            foreach (Vector3Int direction in directions) {
                Vector3Int destination = localCoord + direction;
            
                float directionalSlope;
                if (destination.x < 0 || destination.x >= settings.chunkSize || destination.y < 0 || destination.y >= settings.chunkSize) {
                    destination = GlobalCoordFromLocalCoord(localCoord, chunkCoord) + direction;
                    directionalSlope = EvaluateHeight(destination) - coordHeight;
                }
                else
                    directionalSlope = GetHeight(destination, chunkCoord) - coordHeight;

                if (directionalSlope > slope)
                    slope = directionalSlope;
            }
            return slope;
        }
    }

    public float GetHeight(Vector3Int localCoord, Vector3Int chunkCoord) {
        if (GetComponent<ChunkLoader>().chunks.ContainsKey(chunkCoord))
            return GetComponent<ChunkLoader>().chunks[chunkCoord].GetHeight(localCoord);
        else
            return EvaluateHeight(GlobalCoordFromLocalCoord(localCoord, chunkCoord));
    }

    public float GetTemperature(Vector3Int localCoord, Vector3Int chunkCoord) {
        if (GetComponent<ChunkLoader>().chunks.ContainsKey(chunkCoord))
            return GetComponent<ChunkLoader>().chunks[chunkCoord].GetTemperature(localCoord);
        else
            return EvaluateTemperature(GlobalCoordFromLocalCoord(localCoord, chunkCoord));
    }

    public Vector3Int GetChunkCoordFromPlayerCoord(Vector3 position) {
        float chunkDistance = settings.chunkSize * settings.tileSize;
        int i = Mathf.FloorToInt(position.x/chunkDistance);
        int j = Mathf.FloorToInt(position.y/chunkDistance);
        return new Vector3Int(i, j, 0);
    }

    public Vector3Int GetLocalCoordFromPlayerCoord(Vector3 position) {
        int i = Mathf.FloorToInt(((position.x / settings.tileSize)%settings.chunkSize + settings.chunkSize) % settings.chunkSize);
        int j = Mathf.FloorToInt(((position.y / settings.tileSize)%settings.chunkSize + settings.chunkSize) % settings.chunkSize);
        return new Vector3Int(i, j, 0);
    }

    public Vector3Int GlobalCoordFromLocalCoord(Vector3Int localCoord, Vector3Int chunkCoord) {
        return localCoord + chunkCoord * settings.chunkSize;
    }

    public Vector3Int[] LocalCoordFromGlobalCoord(Vector3Int globalCoord) {
        int i = (globalCoord.x % settings.chunkSize + settings.chunkSize) % settings.chunkSize;
        int j = (globalCoord.y % settings.chunkSize + settings.chunkSize) % settings.chunkSize;
        int x = Mathf.FloorToInt((float) globalCoord.x / settings.chunkSize);
        int y = Mathf.FloorToInt((float) globalCoord.y / settings.chunkSize);
        Vector3Int[] coord = {new Vector3Int(i, j, 0), new Vector3Int(x, y, 0)};
        return coord;
    }
}
