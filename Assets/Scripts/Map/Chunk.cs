using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class Chunk {
    public string name;
    public GameObject chunk;
    public Texture2D chunkTexture;
    public Vector3Int chunkCoord;

    public ChunkData chunkData;
    public bool dataGenerated;
    
    public Settings settings;

    Map map;
    ChunkLoader chunkLoader;

    List<TreeController> trees;

    public Chunk(Vector3Int position, Settings settings, ChunkLoader chunkLoader, Map map) {
        this.settings = settings;
        this.chunkLoader = chunkLoader;
        this.map = map;
        dataGenerated = false;
        CreateChunk(position);
        
        trees = new List<TreeController>();
    }

    public Chunk(Vector3Int position, Settings settings, ChunkLoader chunkLoader, Map map, ChunkData chunkData) {
        this.settings = settings;
        this.chunkLoader = chunkLoader;
        this.map = map;
        this.chunkData = chunkData;
        dataGenerated = true;
        CreateChunk(position);
        
        trees = new List<TreeController>();
    }

    public void CreateChunk(Vector3Int position) {
        chunkTexture = new Texture2D(settings.chunkSize, settings.chunkSize);
        chunkTexture.filterMode = FilterMode.Point;
        Rect rec = new Rect(0, 0, settings.chunkSize, settings.chunkSize);

        name = "Chunk (" + position.x + ", " + position.y + ")";
        chunk = new GameObject(name);
        chunk.transform.SetParent(map.transform);
        SpriteRenderer spriteRenderer = chunk.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = Sprite.Create(chunkTexture, rec, new Vector2(0, 0), 1/settings.tileSize);
        spriteRenderer.sortingLayerName = "Map";

        chunkCoord = position;
        chunk.transform.position = new Vector3(position.x * settings.chunkSize * settings.tileSize, position.y * settings.chunkSize * settings.tileSize, 0);
    }

    public void DrawChunk() {
        Color[] pixels = new Color[settings.chunkSize * settings.chunkSize];
        for (int i = 0; i < settings.chunkSize; i++) {
            for (int j = 0; j < settings.chunkSize; j++) {
                Vector3Int localCoord = new Vector3Int(i, j, 0);
                SetPixel(pixels, localCoord);
            }
        }
        chunkTexture.SetPixels(pixels);
        chunkTexture.Apply();
        PopulateTrees();
    }

    
    public async Task DrawChunkAsync() {
        Color[] pixels = new Color[settings.chunkSize * settings.chunkSize];
        for (int i = 0; i < settings.chunkSize; i++) {
            for (int j = 0; j < settings.chunkSize; j++) {
                Vector3Int localCoord = new Vector3Int(i, j, 0);
                SetPixel(pixels, localCoord);
            }
            await Task.Yield();
        }
        chunkTexture.SetPixels(pixels);
        chunkTexture.Apply();
        PopulateTrees();
    }

    public void GenerateChunkData() {
        dataGenerated = false;
        chunkData = new ChunkData(settings);
        for (int i = 0; i < settings.chunkSize; i++) {
            for (int j = 0; j < settings.chunkSize; j++) {
                Vector3Int localCoord = new Vector3Int(i, j, 0);
                Vector3Int coord = GlobalCoordFromLocalCoord(localCoord);
                chunkData.height[i, j] = map.EvaluateHeight(coord);
                chunkData.slope[i, j] = GetSlope(localCoord);
                chunkData.temperature[i, j] = map.EvaluateTemperature(coord);
                chunkData.humidity[i, j] = map.EvaluateHumidity(coord);
                chunkData.color[i, j] = GetColor(localCoord);
            }
        }
        dataGenerated = true;
        chunkLoader.SaveChunk(chunkCoord);
    }

    public async Task GenerateChunkDataAsync() {
        dataGenerated = false;
        chunkData = new ChunkData(settings);
        for (int i = 0; i < settings.chunkSize; i++) {
            for (int j = 0; j < settings.chunkSize; j++) {
                Vector3Int localCoord = new Vector3Int(i, j, 0);
                Vector3Int coord = GlobalCoordFromLocalCoord(localCoord);
                chunkData.height[i, j] = map.EvaluateHeight(coord);
                chunkData.slope[i, j] = GetSlope(localCoord);
                chunkData.temperature[i, j] = map.EvaluateTemperature(coord);
                chunkData.humidity[i, j] = map.EvaluateHumidity(coord);
                chunkData.color[i, j] = GetColor(localCoord);
            }
            await Task.Yield();
        }
        dataGenerated = true;
        chunkLoader.SaveChunk(chunkCoord);
    }

    public void PopulateTrees() {
        Random.InitState(chunkCoord.x + chunkCoord.y * settings.chunkSize);
        int n = (int) (settings.chunkSize * settings.chunkSize * settings.treeDensity);
        for (int i = 0; i < n; i++) {
            Vector3Int randomPosition = new Vector3Int(Random.Range(0, settings.chunkSize), Random.Range(0, settings.chunkSize), 0);
            Biome biome = GetBiome(randomPosition);
            if (IsPositionValid(randomPosition, biome)) SpawnTree(randomPosition, biome);
        }
    }

    public void RemoveTrees() {
        foreach (TreeController tree in trees) {
            Object.Destroy(tree.gameObject);
        }
    }

    bool IsPositionValid(Vector3Int localCoord, Biome biome) {
        if (biome.trees.Length == 0) return false;
        
        float height = GetHeight(localCoord);
        float norm_height = (height - settings.waterLevel) / (1 - settings.waterLevel);
        if (norm_height < (settings.beachLevel + settings.treeMargin) ||
            norm_height > (settings.peakLevel - settings.treeMargin)) return false;

        float slope = GetSlope(localCoord);
        if (slope > 0.01f) return false;

        if (IsTreeClose(localCoord, biome.treeRange)) return false;
        return Random.Range(0.0f, 1.0f) < biome.treeProbability;
    }

    bool IsTreeClose(Vector3Int position, float range) {
        foreach (TreeController tree in trees) {
            if (Vector3Int.Distance(position, tree.position) < range) return true;
        }
        return false;
    }

    void SpawnTree(Vector3Int position, Biome biome) {
        GameObject treePrefab = biome.trees[Random.Range(0, biome.trees.Length)];
        Vector3 treePosition = GetCoordFromBlockPosition(position); // + new Vector3(settings.tileSize * 0.5f, 0, 0);
        GameObject treeObject = GameObject.Instantiate(treePrefab, treePosition, Quaternion.identity, chunk.transform);
        treeObject.name = "Tree";
        treeObject.GetComponent<TreeController>().position = position;
        trees.Add(treeObject.GetComponent<TreeController>());
    }

    public TreeController GetClosestTree(Vector3 center) {
        TreeController closestTree = null;
        float closestDistance = Mathf.Infinity;
        foreach (TreeController tree in trees) {
            float distance = Vector3.Distance(center, tree.transform.position);
            if (distance < closestDistance) {
                closestDistance = distance;
                closestTree = tree;
            }
        }
        return closestTree;
    }

    public Vector3 GetCoordFromBlockPosition(Vector3Int localBlockPosition) {
        float x = (localBlockPosition.x + chunkCoord.x * settings.chunkSize) * settings.tileSize;
        float y = (localBlockPosition.y + chunkCoord.y * settings.chunkSize) * settings.tileSize;
        return new Vector3(x, y, 0);
    }

    public void SetPixel(Color[] pixels, Vector3Int localCoord) {
        int k = localCoord.x + localCoord.y * settings.chunkSize;
        

        float slope;
        float color;
        switch (map.mapType) {
            case Map.MapType.HeightMap:
                float height = GetHeight(localCoord);
                float norm_height = (height + 1)/2;
                color = (int) (norm_height * 8) / 8.0f;
                if (height < settings.waterLevel)  pixels[k] = new Color(0, 0, color);
                else pixels[k] = new Color(0, color, 0);
                break;
            case Map.MapType.Continentalness:
                float continentalness = (GetContinentalness(localCoord) + 1)/2;
                color = (int) (continentalness * 8) / 8.0f;
                pixels[k] = new Color(color, color, color);
                break;
            case Map.MapType.Erosion:
                float erosion = (GetErosion(localCoord) + 1)/2;
                color = (int) (erosion * 8) / 8.0f;
                pixels[k] = new Color(color, color, color);
                break;
            case Map.MapType.PeaksAndValleys:
                float peaksAndValleys = (GetPeaksAndValleys(localCoord) + 1)/2;
                color = (int) (peaksAndValleys * 8) / 8.0f;
                pixels[k] = new Color(color, color, color);
                break;
            case Map.MapType.Temperature:
                float temperature = (GetTemperature(localCoord) + 1)/2;
                color = (int) (temperature * 8) / 8.0f;
                pixels[k] = Color.Lerp(Color.blue, Color.red, color);
                break;
            case Map.MapType.Humidity:
                float humidity = (GetHumidity(localCoord) + 1)/2;
                color = (int) (humidity * 8) / 8.0f;
                pixels[k] = Color.Lerp(Color.yellow, Color.blue, color);
                break;
            case Map.MapType.Slope:
                slope = GetSlope(localCoord) * 10;
                color = (int) (slope * 8) / 8.0f;
                pixels[k] = new Color(color, color, color);
                break;
            case Map.MapType.Biomes:
                pixels[k] = GetBiomeColor(localCoord);
                break;
            case Map.MapType.NoMerging:
                pixels[k] = GetDirectColor(localCoord);
                break;
            case Map.MapType.Normal:
                pixels[k] = GetColor(localCoord);
                break;
        }
    }

    public Vector3Int GlobalCoordFromLocalCoord(Vector3Int localCoord) {
        return localCoord + chunkCoord * settings.chunkSize;
    }





    /* 
     *   Pixel values
     */

    public float GetContinentalness(Vector3Int localCoord) {
        Vector3Int coord = GlobalCoordFromLocalCoord(localCoord);
        float continentalness = map.EvaluateContinentalness(coord);
        return continentalness;
    }

    public float GetErosion(Vector3Int localCoord) {
        Vector3Int coord = GlobalCoordFromLocalCoord(localCoord);
        return map.EvaluateErosion(coord);
    }

    public float GetPeaksAndValleys(Vector3Int localCoord) {
        Vector3Int coord = GlobalCoordFromLocalCoord(localCoord);
        return map.EvaluatePeaksAndValleys(coord);
    }

    public float GetHeight(Vector3Int localCoord) {
        if (dataGenerated) return chunkData.height[localCoord.x, localCoord.y];
        else  {
            Vector3Int coord = GlobalCoordFromLocalCoord(localCoord);
            return map.EvaluateHeight(coord);
        }
    }

    public float GetSlope(Vector3Int localCoord) {
        if (dataGenerated) return chunkData.slope[localCoord.x, localCoord.y];
        else {
            float slope = 0;
            float coordHeight = GetHeight(localCoord);
            Vector3Int[] directions = {Vector3Int.up, Vector3Int.right, Vector3Int.down, Vector3Int.left};
            foreach (Vector3Int direction in directions) {
                Vector3Int destination = localCoord + direction;
            
                float directionalSlope;
                if (destination.x < 0 || destination.x >= settings.chunkSize || destination.y < 0 || destination.y >= settings.chunkSize) {
                    destination = GlobalCoordFromLocalCoord(localCoord) + direction;
                    directionalSlope = map.EvaluateHeight(destination) - coordHeight;
                }
                else
                    directionalSlope = GetHeight(destination) - coordHeight;

                if (directionalSlope > slope)
                    slope = directionalSlope;
            }
            return slope;
        }
    }

    public float GetTemperature(Vector3Int localCoord) {
        if (dataGenerated) return chunkData.temperature[localCoord.x, localCoord.y];
        else  {
            Vector3Int coord = GlobalCoordFromLocalCoord(localCoord);
            return map.EvaluateTemperature(coord);
        }
    }

    public float GetHumidity(Vector3Int localCoord) {
        if (dataGenerated) return chunkData.humidity[localCoord.x, localCoord.y];
        else  {
            Vector3Int coord = GlobalCoordFromLocalCoord(localCoord);
            return map.EvaluateHumidity(coord);
        }
    }

    public Color GetDirectColor(Vector3Int localCoord) {
        Biome biome = GetBiome(localCoord);
        float height = GetHeight(localCoord);
        float slope = GetSlope(localCoord);
        return biome.GetColor(height, slope);
    }

    public Color GetColor(Vector3Int localCoord) {
        if (dataGenerated) return chunkData.color[localCoord.x, localCoord.y];
        else {
            List<Biome> possibleBiomes = new List<Biome>();
            float temperature = GetTemperature(localCoord);
            float humidity = GetHumidity(localCoord);
            foreach (Biome biome in map.biomes) {
                if (biome.IsAlmostValid(temperature, humidity)) {
                    possibleBiomes.Add(biome);
                }
            }

            if (possibleBiomes.Count == 1) {
                float height = GetHeight(localCoord);
                float slope = GetSlope(localCoord);
                return possibleBiomes[0].GetColor(height, slope);
            }

            float red, green, blue;
            red = green = blue = 0;
            foreach (Biome biome in possibleBiomes) {
                float height = GetHeight(localCoord);
                float slope = GetSlope(localCoord);
                Color color = biome.GetColor(height, slope);
                red += color.r;
                green += color.g;
                blue += color.b;
            }
            return new Color(red / possibleBiomes.Count, green / possibleBiomes.Count, blue / possibleBiomes.Count);
        }
    }

    public Biome GetBiome(Vector3Int localCoord) {
        float temperature = GetTemperature(localCoord);
        float humidity = GetHumidity(localCoord);
        foreach (Biome biome in map.biomes) {
            if (biome.enabled && biome.IsValid(temperature, humidity))
                return biome;
        }
        return null;
    }

    public Color GetBiomeColor(Vector3Int localCoord) {
        Biome biome = GetBiome(localCoord);
        float height = GetHeight(localCoord);
        if (height < settings.waterLevel) return Color.cyan;
        else return biome.biomeColor;
    }
}
