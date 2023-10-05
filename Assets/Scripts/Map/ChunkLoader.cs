using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class ChunkLoader : MonoBehaviour {

    public static ChunkLoader instance;

    public PlayerController player;

    public Dictionary<Vector3Int, Chunk> chunks;
    private Settings settings;
    [HideInInspector]
    public float progress;
    [HideInInspector]
    public bool isDone;

    Vector3Int initialChunk;
    int chunksTotalInitialisation;
    int chunksLoaded;

    List<Vector3Int> chunksToLoad;
    List<Vector3Int> chunksToUnload;

    void Awake() {
        instance = this;

        if (MapInfo.mapName == null) {
            MapInfo.mapName = "test";
        }
        
        Map map = GetComponent<Map>();
        settings = map.settings;
        chunks = new Dictionary<Vector3Int, Chunk>();

        int size = settings.initialChunkLoadDistance * 2 + 1;
        chunksTotalInitialisation = size*size;
        chunksLoaded = 0;

        map.LoadMap();
        
        chunksToLoad = new List<Vector3Int>();
        chunksToUnload = new List<Vector3Int>();
    }

    void Start() {
        initialChunk = player.currentChunk;
        LoadMap();
        
        LoadCloseChunksAsync();
        UnloadFarChunksAsync();
    }

    public async void LoadMap() {
        SaveSystem.Init();
        for (int i = -settings.initialChunkLoadDistance; i <= settings.initialChunkLoadDistance; i++) {
            for (int j = -settings.initialChunkLoadDistance; j <= settings.initialChunkLoadDistance; j++) {
                LoadChunk(new Vector3Int(i, j, 0) + initialChunk);
                chunksLoaded++;
                progress = (float) chunksLoaded / (float) chunksTotalInitialisation;
                await Task.Yield();
            }
        }

        isDone = true;
    }

    public async void LoadCloseChunksAsync() {
        while (this != null) {
            if (isDone) {
                Vector3Int closestChunk = GetClosestUnloadedChunk(player.currentChunk);
                if (Vector3Int.Distance(player.currentChunk, closestChunk) <= settings.chunkLoadDistance &&
                    !chunksToLoad.Contains(closestChunk))
                    chunksToLoad.Add(closestChunk);

                if (chunksToLoad.Count > 0) {
                    LoadChunkAsync(chunksToLoad[0]);
                    chunksToLoad.RemoveAt(0);
                }
            }
            await Task.Yield();
        }
    }

    public async void UnloadFarChunksAsync() {
        while (this != null) {
            if (isDone) {
                Vector3Int farthestChunk = GetFarthestLoadedChunk(player.currentChunk);
                if (Vector3Int.Distance(player.currentChunk, farthestChunk) > settings.chunkUnloadDistance &&
                    !chunksToUnload.Contains(farthestChunk)) 
                    chunksToUnload.Add(farthestChunk);

                if (chunksToUnload.Count > 0) {
                    UnloadChunk(chunksToUnload[0]);
                    chunksToUnload.RemoveAt(0);
                }
            }
            await Task.Yield();
        }
    }

    public void SaveChunk(Vector3Int chunkCoord) {
        string chunkData = chunks[chunkCoord].chunkData.GetString();
        string filename = chunkCoord.x + "," + chunkCoord.y;
        SaveSystem.Save(chunkData, filename);
    }

    public ChunkData LoadChunkData(Vector3Int chunkCoord) {
        string filename = chunkCoord.x + "," + chunkCoord.y;
        string chunkData = SaveSystem.Load(filename);
        if (chunkData != null) return new ChunkData(settings, chunkData);
        else return null;
    }

    public Chunk GetChunkOrNull(Vector3Int chunkCoord) {
        Chunk chunk;
        bool chunkFound;
        chunkFound = chunks.TryGetValue(chunkCoord, out chunk);
        if (chunkFound) return chunk;
        return null;
    }

    public Chunk GetChunk(Vector3Int chunkCoord) {
        Chunk chunk;
        bool chunkFound;
        chunkFound = chunks.TryGetValue(chunkCoord, out chunk);
        if (chunkFound) return chunk;

        ChunkData chunkData = LoadChunkData(chunkCoord);
        if (chunkData != null) return new Chunk(chunkCoord, settings, this, GetComponent<Map>(), chunkData);
        else return new Chunk(chunkCoord, settings, this, GetComponent<Map>());
    }

    public void LoadChunk(Vector3Int chunkCoord) {
        Chunk chunk = GetChunk(chunkCoord);
        if (!chunks.ContainsKey(chunkCoord)) chunks.Add(chunkCoord, chunk);
        
        if (!chunk.dataGenerated) chunk.GenerateChunkData();
        chunk.DrawChunk();
    }

    public async void LoadChunkAsync(Vector3Int chunkCoord) {
        Chunk chunk = GetChunk(chunkCoord);
        if (!chunks.ContainsKey(chunkCoord)) chunks.Add(chunkCoord, chunk);
        
        if (!chunk.dataGenerated) await chunk.GenerateChunkDataAsync();
        await chunk.DrawChunkAsync();
    }

    public void UnloadChunk(Vector3Int chunkCoord) {
        if (chunks.ContainsKey(chunkCoord)) {
            chunks[chunkCoord].RemoveTrees();
            Destroy(chunks[chunkCoord].chunk);
            chunks.Remove(chunkCoord);
        }
    }

    public Vector3Int GetClosestUnloadedChunk(Vector3Int center) {
        Vector3Int closest = center;
        float minDistance = Mathf.Infinity;
        for (int i = -settings.chunkLoadDistance*2; i <= settings.chunkLoadDistance*2; i++) {
            for (int j = -settings.chunkLoadDistance*2; j <= settings.chunkLoadDistance*2; j++) {
                Vector3Int chunkCoord = center + new Vector3Int(i, j, 0);
                if (!chunks.ContainsKey(chunkCoord)) {
                    float distance = Vector3Int.Distance(center, chunkCoord);
                     if (distance < minDistance) {
                        closest = chunkCoord;
                        minDistance = distance;
                    }
                }
            }
        }
        return closest;
    }

    public Vector3Int GetFarthestLoadedChunk(Vector3Int center) {
        Vector3Int farthestPosition = center;
        float farthestDistance = 0;
        Vector3Int[] positions = new Vector3Int[chunks.Keys.Count];
        chunks.Keys.CopyTo(positions, 0);
        foreach (Vector3Int position in positions) {
            float currentDistance = Vector3Int.Distance(position, center);
            if (currentDistance > farthestDistance) {
                farthestPosition = position;
                farthestDistance = currentDistance;
            }
        }
        return farthestPosition;
    }
}


