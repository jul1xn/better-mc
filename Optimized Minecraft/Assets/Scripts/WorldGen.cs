using TMPro;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using System.Linq;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine.UIElements;

public class WorldGen : MonoBehaviour
{
    public static WorldGen instance;
    public GameObject cubePrefab;
    public GameObject lightPrefab;
    public Material sharedMaterial;
    [Header("World settings")]
    public float seed;
    public short grassBlock;
    public short dirtBlock;
    public short stoneBlock;
    [Space]
    public float chunkDistance = 20;
    public float chunkDestroyDistance = 50;
    public int chunkRendDistance = 2;
    public int chunksProcessedPerFrame = 1;
    public int chunkSize = 32;
    public float caveScale = 0.05f;       // Controls how frequent caves appear
    public float caveThreshold = 0.4f;    // Controls how "empty" caves are — lower = fewer caves, higher = more caves
    public static int maxHeight = 10;
    public int worldMaxHeight = 100;
    public float noiseScale = 0.1f;
    public Transform player;
    public bool debugDraw = false;
    System.Random randomNumber = new System.Random();

    public Dictionary<Vector2, GameObject> loadedChunks = new Dictionary<Vector2, GameObject>();
    private Vector2 lastPlayerChunk;
    private int atlasWidth;
    public Dictionary<short, Vector2> textureAtlas = new Dictionary<short, Vector2>();

    public ConcurrentQueue<StagedChunk> stagedChunks = new ConcurrentQueue<StagedChunk>();
    public WorldType type;
    private Dictionary<string, Dictionary<string, short>> modifiedBlockCopy;

    private void OnDrawGizmos()
    {
        if (debugDraw && !Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            for (int x = -chunkRendDistance/2; x <= chunkRendDistance/2; x++)
            {
                for (int z = -chunkRendDistance/2; z <= chunkRendDistance/2; z++)
                {
                    Vector3 chunkPos = new Vector3(x * chunkSize, 0, z * chunkSize);
                    Gizmos.DrawWireCube(chunkPos, new Vector3(chunkSize, chunkSize, chunkSize));
                }
            }
        }
    }

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        modifiedBlockCopy = LevelController.instance.t_worldsave.modifiedChunks;
        seed = LevelController.instance.t_worldsave.seed;
        chunkRendDistance = LevelController.instance.s_rendDist;
        atlasWidth = TextureManager.instance.textureAtlas.width;
        textureAtlas = TextureManager.instance.textures;
        type = (WorldType)LevelController.instance.t_worldsave.worldType;

        if (type == WorldType.Normal)
        {
            maxHeight = 50;
        }
        if (type == WorldType.Amplified)
        {
            maxHeight = 150;
        }
        if (type == WorldType.Debug)
        {
            TestTextureAtlas();
        }

        if (!LevelController.instance.w_infiniteWorld)
        {
            chunkRendDistance = (int)LevelController.instance.w_chunkSize;
        }

        if (player == null)
        {
            UnityEngine.Debug.LogError("Player transform not assigned to WorldGen!");
            return;
        }
        lastPlayerChunk = GetChunkCoord(player.position);

        PlayerMovement.instance.TeleportToPosition(WorldSave.ConvertStringToVector3(LevelController.instance.t_worldsave.playerPosition));
        PlayerMovement.instance.mouseLook.SetRotation(WorldSave.ConvertStringToVector3(LevelController.instance.t_worldsave.playerRotation));

        LoadChunksAroundPlayer();
    }


    public void TestTextureAtlas()
    {
        int index = 0;
        float uvSize = 1.0f / (atlasWidth / 16); // Assuming each texture is 16x16 in the atlas

        foreach (var texture in TextureManager.instance.textures)
        {
            int blockID = texture.Key;
            Vector2 uvBase = texture.Value;

            // Create a cube
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"{blockID} - {uvBase}";
            cube.transform.position = new Vector3(index % 10, index / 10, 0); // Arrange in a grid
            cube.transform.localScale = Vector3.one * 0.9f; // Slightly shrink for spacing

            // Assign material
            MeshRenderer renderer = cube.GetComponent<MeshRenderer>();
            renderer.material = sharedMaterial;

            // Adjust UV mapping
            Mesh mesh = cube.GetComponent<MeshFilter>().mesh;
            Vector2[] uvs = new Vector2[mesh.uv.Length];

            // Correct UV mapping based on atlas coordinates
            uvs[0] = uvBase + new Vector2(0, 0);
            uvs[1] = uvBase + new Vector2(uvSize, 0);
            uvs[2] = uvBase + new Vector2(0, uvSize);
            uvs[3] = uvBase + new Vector2(uvSize, uvSize);

            mesh.uv = uvs;

            index++;
        }
    }
    private void Update()
    {
        ProcessStagedChunks();
        Vector2 currentChunk = GetChunkCoord(player.position);
        if (currentChunk != lastPlayerChunk && LevelController.instance.w_infiniteWorld)
        {
            lastPlayerChunk = currentChunk;
            LoadChunksAroundPlayer();
            UnloadDistantChunks();
        }
    }

    private Vector2 GetChunkCoord(Vector3 position)
    {
        return new Vector2(Mathf.FloorToInt(position.x / chunkSize) * chunkSize, Mathf.FloorToInt(position.z / chunkSize) * chunkSize);
    }

    private void UnloadDistantChunks()
    {
        List<Vector2> chunksToRemove = new List<Vector2>();

        foreach (var chunk in loadedChunks)
        {
            float distance = Vector2.Distance(chunk.Key, lastPlayerChunk);

            if (distance > (chunkRendDistance * chunkSize) && loadedChunks[chunk.Key] != null)
            {
                chunksToRemove.Add(chunk.Key);
            }
        }

        foreach (var chunkPos in chunksToRemove)
        {
            if (loadedChunks.ContainsKey(chunkPos) && loadedChunks[chunkPos] != null)
            {
                Destroy(loadedChunks[chunkPos]);
                loadedChunks.Remove(chunkPos);
            }
        }
    }



    private void LoadChunksAroundPlayer()
    {
        if (type == WorldType.Debug) { return; }
        List<Vector2> newChunks = new List<Vector2>();

        for (int x = -chunkRendDistance/2; x <= chunkRendDistance/2; x++)
        {
            for (int z = -chunkRendDistance/2; z <= chunkRendDistance/2; z++)
            {
                Vector2 chunkPos = lastPlayerChunk + new Vector2(x * chunkSize, z * chunkSize);
                if (!loadedChunks.ContainsKey(chunkPos))
                {
                    newChunks.Add(chunkPos);
                }
            }
        }

        newChunks.Sort((a, b) => Vector2.Distance(a, lastPlayerChunk).CompareTo(Vector2.Distance(b, lastPlayerChunk)));

        foreach (var chunkPos in newChunks)
        {
            GenerateChunk(chunkPos);
        }
    }


    public static float GetNoise(float seed, float x, float z)
    {
        float xOffset = seed * 1000;
        float zOffset = seed * 1000;

        float noise1 = Mathf.PerlinNoise((x + xOffset) * 3f, (z + zOffset) * 3f);
        float noise2 = Mathf.PerlinNoise((x + xOffset) * 6f, (z + zOffset) * 6f) * 0.5f;
        float noise3 = Mathf.PerlinNoise((x + xOffset) * 12f, (z + zOffset) * 12f) * 0.25f;
        float noise4 = Mathf.PerlinNoise((x + xOffset) * 24f, (z + zOffset) * 24f) * 0.125f;

        return noise1 + noise2 + noise3 + noise4;
    }

    float Get3DNoise(float x, float y, float z)
    {
        x *= caveScale;
        y *= caveScale;
        z *= caveScale;

        float xy = Mathf.PerlinNoise(x, y);
        float yz = Mathf.PerlinNoise(y, z);
        float xz = Mathf.PerlinNoise(x, z);
        float yx = Mathf.PerlinNoise(y, x);
        float zy = Mathf.PerlinNoise(z, y);
        float zx = Mathf.PerlinNoise(z, x);

        return (xy + yz + xz + yx + zy + zx) / 6f;
    }


    private void ProcessStagedChunks()
    {
        if (stagedChunks.Count > 0)
        {
            int processed = 0;
            while (processed < chunksProcessedPerFrame && stagedChunks.TryDequeue(out StagedChunk stagedChunk))
            {
                if (loadedChunks.ContainsKey(stagedChunk.chunkPosition))
                {
                    UnityEngine.Debug.LogWarning($"Chunk {stagedChunk.chunkPosition} is already loaded, skipping generation.");
                    return;
                }

                GameObject chunk = new GameObject($"{stagedChunk.chunkPosition.x};{stagedChunk.chunkPosition.y}");
                chunk.transform.parent = transform;
                chunk.layer = gameObject.layer;
                Chunk c = chunk.AddComponent<Chunk>();
                c.cubes = stagedChunk.cubePositions;
                c._chunkPos = WorldSave.ConvertVector2ToString(stagedChunk.chunkPosition);

                Mesh mesh = new Mesh();
                mesh.vertices = stagedChunk.vertices;
                mesh.triangles = stagedChunk.triangles;
                mesh.uv = stagedChunk.uvs;
                mesh.RecalculateNormals();


                MeshCollider meshCollider = chunk.GetComponent<MeshCollider>();
                if (meshCollider == null)
                {
                    meshCollider = chunk.AddComponent<MeshCollider>();
                }

                MeshFilter meshFilter = chunk.GetComponent<MeshFilter>();
                if (meshFilter == null)
                {
                    meshFilter = chunk.AddComponent<MeshFilter>();
                }

                MeshRenderer rend = chunk.GetComponent<MeshRenderer>();
                if (rend == null)
                {
                    rend = chunk.AddComponent<MeshRenderer>();
                    rend.material = sharedMaterial;
                }

                foreach(var kvp in stagedChunk.lightLevels)
                {
                    Light l = Instantiate(lightPrefab, kvp.Key, Quaternion.identity).GetComponent<Light>();
                    l.transform.parent = chunk.transform;
                }


                meshFilter.sharedMesh = mesh;
                meshCollider.sharedMesh = mesh;
                loadedChunks[stagedChunk.chunkPosition] = chunk;
                processed++;
            }
        }
    }

    public int activeThreads = 0;
    public int maxConcurrentThreads = 2; // Tune this number based on performance

    private void GenerateChunk(Vector2 chunkPos)
    {
        if (loadedChunks.ContainsKey(chunkPos))
        {
            UnityEngine.Debug.LogWarning($"Chunk {chunkPos} is already loaded, skipping generation.");
            return;
        }

        Interlocked.Increment(ref activeThreads);
        ThreadPool.QueueUserWorkItem(state =>
        {
            ChunkThread(chunkPos);
            Interlocked.Decrement(ref activeThreads);
        });
    }


    private void ChunkThread(Vector2 chunkPos)
    {
        Log($"[ChunkThread] Starting chunk generation at {chunkPos}");
        Dictionary<Vector3, short> cubes = new Dictionary<Vector3, short>();
        string chunkKey = WorldSave.ConvertVector2ToString(chunkPos);

        Log($"[ChunkThread] Calculating terrain height for chunk {chunkPos}");
        for (int x = (int)chunkPos.x; x < chunkPos.x + chunkSize; x++)
        {
            for (int z = (int)chunkPos.y; z < chunkPos.y + chunkSize; z++)
            {
                float noiseValue = GetNoise(seed, x * noiseScale, z * noiseScale);
                int height = Mathf.RoundToInt(noiseValue * maxHeight);
                int stoneHeight = (int)(height * 0.85f);

                Log($"[ChunkThread] Noise at ({x}, {z}): {noiseValue}, height: {height}, stoneHeight: {stoneHeight}");

                for (int y = 0; y < height; y++)
                {
                    Vector3 pos = new Vector3(x, y, z);

                    float caveNoise = Get3DNoise(x, y, z);
                    if (caveNoise < caveThreshold)
                    {
                        continue; // Skip block to make a cave
                    }

                    if (y <= stoneHeight)
                        cubes[pos] = stoneBlock;
                    else if (y == height - 1)
                        cubes[pos] = grassBlock;
                    else
                        cubes[pos] = dirtBlock;
                }


                System.Random rng = new System.Random((int)(seed + x * 73856093 + z * 19349663)); // Unique seed per chunk
                foreach(Structure s in BlocksManager.Instance.allStructures)
                {
                    Vector3 basePos = new Vector3(x, height, z);

                    if (rng.Next(s.commonness) == 0 && cubes.ContainsKey(new Vector3(x, height - 1, z)))
                    {
                        foreach (StructureBlock block in s.blocks)
                        {
                            Vector3 targetPos = basePos + block.relativePosition;

                            //if (targetPos.x < chunkPos.x || targetPos.x >= chunkPos.x + chunkSize ||
                            //    targetPos.z < chunkPos.y || targetPos.z >= chunkPos.y + chunkSize)
                            //{
                            //    continue;
                            //}

                            cubes[targetPos] = block.blockId;
                        }
                    }
                }
            }
        }

        if (modifiedBlockCopy.TryGetValue(chunkKey, out var modifications))
        {
            Log($"[ChunkThread] Chunk {chunkPos} is modified");
            var localCopy = new Dictionary<string, short>(modifications);
            Log($"[ChunkThread] Modified block count: {localCopy.Count}");

            foreach (var kvp in localCopy)
            {
                Vector3 pos = WorldSave.ConvertStringToVector3(kvp.Key);
                Log($"[ChunkThread] Modifying block at {pos} to {kvp.Value}");
                if (kvp.Value == -1)
                {
                    // Remove the block (set to air)
                    cubes.Remove(pos);
                }
                else
                {
                    cubes[pos] = kvp.Value; // Replace or add the block
                }
            }
        }



        Log($"[ChunkThread] Starting face detection for chunk {chunkPos}");
        List<(Vector3, Quaternion, int, Vector2, Vector2)> faceData = new List<(Vector3, Quaternion, int, Vector2, Vector2)>();
        HashSet<Vector3> cubeSet = new HashSet<Vector3>(cubes.Keys);
        Dictionary<Vector3, byte> lightLevels = new Dictionary<Vector3, byte>();
         
        foreach (var cube in cubes)
        {
            byte lightLevel = BlocksManager.Instance.GetLightLevel((int)cube.Value);
            CheckFaces(cube.Key, faceData, cubeSet, cubes[cube.Key], lightLevel, lightLevels);
        }
        Log($"[ChunkThread] Face detection complete. Total faces: {faceData.Count}");

        Log($"[ChunkThread] Generating mesh data for chunk {chunkPos}");
        (Vector3[], int[], Vector2[]) meshData = GenerateChunkCollider(faceData);
        Log($"[ChunkThread] Mesh data generated: Vertices: {meshData.Item1.Length}, Triangles: {meshData.Item2.Length}, UVs: {meshData.Item3.Length}");


        StagedChunk chunk = new StagedChunk();
        chunk.lightLevels = lightLevels;
        chunk.vertices = meshData.Item1;
        chunk.triangles = meshData.Item2;
        chunk.uvs = meshData.Item3;
        chunk.chunkPosition = chunkPos;
        chunk.cubePositions = cubes;

        stagedChunks.Enqueue(chunk);
        Log($"[ChunkThread] Chunk {chunkPos} enqueued for rendering");
    }


    public void CheckFaces(Vector3 cubePosition, List<(Vector3, Quaternion, int, Vector2, Vector2)> faceData, HashSet<Vector3> cubes, short blockId, byte lightLevel, Dictionary<Vector3, byte> lightLevelData)
    {
        Block block = BlocksManager.Instance.allBlocks[blockId];

        (Vector3 direction, Quaternion rotation, string face)[] checks =
            {
            (Vector3.forward, Quaternion.identity, "front"),
            (Vector3.back, Quaternion.Euler(0, 180, 0), "back"),
            (Vector3.right, Quaternion.Euler(0, 90, 0), "right"),
            (Vector3.left, Quaternion.Euler(0, -90, 0), "left"),
            (Vector3.down, Quaternion.Euler(90, 0, 0), "bottom"),
            (Vector3.up, Quaternion.Euler(-90, 0, 0), "top")
        };

        foreach (var (direction, rotation, face) in checks)
        {
            if (!cubes.Contains(cubePosition + direction))
            {
                short textureIndex = BlocksManager.GetTextureIndexForFace(block, face);
                Vector2 uvBase = textureAtlas[textureIndex];

                faceData.Add((cubePosition + direction * 0.5f, rotation, blockId, uvBase, new Vector2(1, 1)));
                //if (lightLevel > 0)
                //{
                //    lightLevelData[cubePosition + direction * 0.7f] = lightLevel;
                //}
            }
        }
    }


    public (Vector3[], int[], Vector2[]) GenerateChunkCollider(List<(Vector3, Quaternion, int, Vector2, Vector2)> faceData)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        Dictionary<(Vector3, Quaternion), List<(Vector3, Vector2)>> mergedFaces = new Dictionary<(Vector3, Quaternion), List<(Vector3, Vector2)>>();

        foreach (var (facePos, rotation, blockId, uvBase, size) in faceData)
        {
            var key = (facePos, rotation);
            if (!mergedFaces.ContainsKey(key))
                mergedFaces[key] = new List<(Vector3, Vector2)>();

            mergedFaces[key].Add((facePos, uvBase));
        }

        foreach (var ((facePos, rotation), faces) in mergedFaces)
        {
            int index = vertices.Count;
            Vector3[] baseVertices =
            {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3(0.5f, -0.5f, 0),
            new Vector3(0.5f, 0.5f, 0),
            new Vector3(-0.5f, 0.5f, 0)
        };

            for (int i = 0; i < baseVertices.Length; i++)
            {
                vertices.Add(facePos + rotation * baseVertices[i]);
            }

            triangles.AddRange(new int[] { index, index + 1, index + 2, index, index + 2, index + 3 });

            float uvSize = 1.0f / (atlasWidth / 16);
            uvs.Add(faces[0].Item2);
            uvs.Add(faces[0].Item2 + new Vector2(uvSize, 0));
            uvs.Add(faces[0].Item2 + new Vector2(uvSize, uvSize));
            uvs.Add(faces[0].Item2 + new Vector2(0, uvSize));
        }

        return (vertices.ToArray(), triangles.ToArray(), uvs.ToArray());
    }

    public void Log(string msg)
    {
        UnityEngine.Debug.Log("[WorldGen] " + msg);
    }
}

[Serializable]
public class StagedChunk
{
    public Vector2 chunkPosition;
    public Dictionary<Vector3, byte> lightLevels;
    public Dictionary<Vector3, short> cubePositions;
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;
}

public enum WorldType { Normal, Flat, Amplified, Debug }