using TMPro;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using System.Linq;

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

        PlayerMovement.instance.TeleportToPosition(Helper.ConvertStringToVector3(LevelController.instance.t_worldsave.playerPosition));
        PlayerMovement.instance.mouseLook.SetRotation(Helper.ConvertStringToVector3(LevelController.instance.t_worldsave.playerRotation));

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
            Vector2[] uvs = mesh.uv;

            // Update the UVs using base + offset
            uvs[0] = uvBase + new Vector2(0, 0);
            uvs[1] = uvBase + new Vector2(uvSize, 0);
            uvs[2] = uvBase + new Vector2(0, uvSize);
            uvs[3] = uvBase + new Vector2(uvSize, uvSize);
            uvs[4] = uvBase + new Vector2(0, 0);
            uvs[5] = uvBase + new Vector2(uvSize, 0);
            uvs[6] = uvBase + new Vector2(0, uvSize);
            uvs[7] = uvBase + new Vector2(uvSize, uvSize);

            mesh.uv = uvs;

            // Create 3D Text for the blockID
            GameObject textObj = new GameObject($"Text_{blockID}");
            textObj.transform.SetParent(cube.transform); // Parent to cube
            textObj.transform.localPosition = new Vector3(0, 0f, 0.6f); // Slightly above the cube
            textObj.transform.localScale = Vector3.one * 0.85f;
            textObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 180, 0));

            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = blockID.ToString();
            textMesh.fontSize = 32;
            textMesh.characterSize = 0.1f;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.color = Color.black;

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
        int processed = 0;

        while (processed < chunksProcessedPerFrame && stagedChunks.TryDequeue(out StagedChunk stagedChunk))
        {
            Vector2 chunkPos = stagedChunk.chunkPosition;
            if (loadedChunks.ContainsKey(chunkPos))
            {
                LogWarning($"[ProcessStagedChunks] Chunk {chunkPos} already loaded, skipping.");
                continue;
            }

            string chunkName = $"{chunkPos.x};{chunkPos.y}";

            GameObject chunkObj = new GameObject(chunkName)
            {
                layer = gameObject.layer,
                transform = { parent = transform }
            };

            MeshFilter meshFilter = chunkObj.AddComponent<MeshFilter>();
            MeshRenderer renderer = chunkObj.AddComponent<MeshRenderer>();
            MeshCollider collider = chunkObj.AddComponent<MeshCollider>();
            Chunk chunk = chunkObj.AddComponent<Chunk>();

            chunk._chunkPos = Helper.ConvertVector2ToString(chunkPos);
            chunk.cubes = stagedChunk.cubePositions;

            Mesh mesh = new Mesh
            {
                vertices = stagedChunk.vertices,
                triangles = stagedChunk.triangles,
                uv = stagedChunk.uvs,
                colors = stagedChunk.colors
            };
            mesh.RecalculateNormals();

            meshFilter.sharedMesh = mesh;
            collider.sharedMesh = mesh;
            renderer.material = sharedMaterial;

            foreach (var kvp in stagedChunk.lightLevels)
            {
                GameObject lightObj = Instantiate(lightPrefab, kvp.Key, Quaternion.identity, chunkObj.transform);
                lightObj.name = $"Light_{kvp.Key}";
            }

            loadedChunks[chunkPos] = chunkObj;
            processed++;

            GameObject old = GameObject.Find(chunkName);
            if (old != null && old != chunkObj)
            {
                Destroy(old);
            }

            Log($"[ProcessStagedChunks] Chunk {chunkName} added successfully.");
        }
    }


    public int activeThreads = 0;
    public int maxConcurrentThreads = 2;

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

    private void GenerateTerrain(Vector2 chunkPos, ref Dictionary<Vector3, short> cubes)
    {
        for (int x = (int)chunkPos.x; x < chunkPos.x + chunkSize; x++)
        {
            for (int z = (int)chunkPos.y; z < chunkPos.y + chunkSize; z++)
            {
                Biome b = BlocksManager.Instance.GetBiomeAtPos(new Vector2(x, z), seed);
                float noiseValue = GetNoise(seed, x * noiseScale, z * noiseScale);
                int height = Mathf.RoundToInt(noiseValue * maxHeight);
                int stoneHeight = (int)(height * 0.85f);

                for (int y = -30; y < height; y++)
                {
                    Vector3 pos = new Vector3(x, y, z);

                    float caveNoise = Get3DNoise(x, y + 30, z);
                    if (caveNoise < caveThreshold)
                    {
                        continue;
                    }

                    if (y <= stoneHeight)
                        cubes[pos] = b.stoneBlockId;
                    else if (y == height - 1)
                        cubes[pos] = b.grassBlockId;
                    else
                        cubes[pos] = b.dirtBlockId;
                }

                cubes[new Vector3(x, -31, z)] = 9;

                System.Random rng = new System.Random((int)(seed + x * 73856093 + z * 19349663));

                foreach (SpawnableFeature s in b.spawnAbleFeatures)
                {
                    Vector3 basePos = new Vector3(x, height, z);

                    if (rng.Next(s.commonness) == 0 && cubes.ContainsKey(new Vector3(x, height - 1, z)))
                    {
                        foreach (FeatureBlock block in s.feature.blocks)
                        {
                            Vector3 targetPos = basePos + block.relativePosition;
                            cubes[targetPos] = block.blockId;
                        }
                    }
                }
            }
        }
    }

    private void ApplyModifiedBlocks(Vector2 chunkPos, ref Dictionary<Vector3, short> cubes)
    {
        string chunkKey = Helper.ConvertVector2ToString(chunkPos);
        if (modifiedBlockCopy.TryGetValue(chunkKey, out var modifications))
        {
            var localCopy = new Dictionary<string, short>(modifications);

            foreach (var kvp in localCopy)
            {
                Vector3 pos = Helper.ConvertStringToVector3(kvp.Key);
                if (kvp.Value == -1)
                {
                    cubes.Remove(pos);
                }
                else
                {
                    cubes[pos] = kvp.Value;
                }
            }
        }
    }


    public void CheckFaces(
            Vector3 cubePosition,
            List<(Vector3, Quaternion, int, Vector2, Vector2)> faceData,
            Dictionary<Vector3, short> cubes,
            short blockId,
            byte lightLevel,
            Dictionary<Vector3, byte> lightLevelData,
            List<Color> colors)
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
            bool shouldAddFace = false;

            if (!cubes.TryGetValue(cubePosition + direction, out short otherCube))
            {
                shouldAddFace = true;
            }
            else if (BlocksManager.Instance.IsTransparent(otherCube))
            {
                shouldAddFace = true;
            }

            if (shouldAddFace)
            {
                short textureIndex = BlocksManager.GetTextureIndexForFace(block, face);
                Vector2 uvBase = textureAtlas[textureIndex];

                System.Random r = new System.Random();

                Color faceColor = Color.white;

                if (BlocksManager.Instance.IsFoiliage(blockId))
                {
                    Biome currentBiome = BlocksManager.Instance.GetBiomeAtPos(new Vector2(cubePosition.x, cubePosition.z), seed);
                    faceColor = currentBiome.foiliageColor;
                }

                // Add the color to your colors list
                colors.Add(faceColor); colors.Add(faceColor); colors.Add(faceColor); colors.Add(faceColor);

                faceData.Add((cubePosition + direction * 0.5f, rotation, blockId, uvBase, new Vector2(1, 1)));

                if (lightLevel > 0)
                {
                    lightLevelData[cubePosition + direction * 0.7f] = lightLevel;
                }
            }
        }
    }


    private List<(Vector3, Quaternion, int, Vector2, Vector2)> CreateFaces(ref Dictionary<Vector3, short> cubes, ref Dictionary<Vector3, byte> lightLevels, ref List<Color> colors)
    {
        List<(Vector3, Quaternion, int, Vector2, Vector2)> faceData = new List<(Vector3, Quaternion, int, Vector2, Vector2)>();
        foreach (var cube in cubes)
        {
            byte lightLevel = BlocksManager.Instance.GetLightLevel((int)cube.Value);
            CheckFaces(cube.Key, faceData, cubes, cubes[cube.Key], lightLevel, lightLevels, colors);
        }

        return faceData;
    }


    private void ChunkThread(Vector2 chunkPos)
    {
        try
        {
            List<Color> colors = new List<Color>();
            Dictionary<Vector3, short> cubes = new Dictionary<Vector3, short>();
            Dictionary<Vector3, byte> lightLevels = new Dictionary<Vector3, byte>();

            GenerateTerrain(chunkPos, ref cubes);
            ApplyModifiedBlocks(chunkPos, ref cubes);

            var faceData = CreateFaces(ref cubes, ref lightLevels, ref colors);
            var (verts, tris, uvs) = GenerateChunkCollider(faceData);

            // Enqueue result
            StagedChunk chunk = new StagedChunk
            {
                lightLevels = lightLevels,
                vertices = verts,
                triangles = tris,
                uvs = uvs,
                chunkPosition = chunkPos,
                cubePositions = cubes,
                colors = colors.ToArray()
            };

            stagedChunks.Enqueue(chunk);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"[ChunkThread ERROR] Chunk {chunkPos}: {ex.Message}\n{ex.StackTrace}");
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
        if (debugDraw)
        {
            UnityEngine.Debug.Log("[WorldGen] " + msg);
        }
    }

    public void LogWarning(string msg)
    {
        if (debugDraw)
        {
            UnityEngine.Debug.LogWarning("[WorldGen] " + msg);
        }
    }

    // For Debugging purposes
    private static readonly object csvLock = new object();
    private const string ChunkTimingCsvPath = "ChunkGenerationTimings.csv";

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
    public Color[] colors;
}

public enum WorldType { Normal, Flat, Amplified, Debug }