using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Rendering.PostProcessing;

public class Chunk : MonoBehaviour
{
    MeshRenderer rend;
    public Transform targetCube;
    private HashSet<Vector3> cubePositions;
    public float distance;
    public float destroyDistance;
    Vector3 chunkCenter;
    public Dictionary<Vector3, short> cubes;
    public string _chunkPos;
    public Vector2 _chunk;

    Vector3 pos1;
    Vector3 pos2;
    Bounds bounds;

    Vector3[] worldCorners;

    private void Start()
    {
        distance = WorldGen.instance.chunkDistance;
        rend = GetComponent<MeshRenderer>();
        // Calculate the chunk center manually
        float chunkSize = WorldGen.instance.chunkSize;
        string[] lines = gameObject.name.Split(";");
        _chunk = new Vector2(int.Parse(lines[0]), int.Parse(lines[1]));
        Vector3 actualPos = new Vector3(_chunk.x, transform.position.y, _chunk.y);
        chunkCenter = actualPos + new Vector3(chunkSize / 2f, chunkSize / 2f, chunkSize / 2f);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return; // Avoid running in edit mode

        // Get player position
        Vector3 playerPosition = PlayerMovement.instance.transform.position;

        // Calculate distance from player to chunk center
        float distanceToPlayer = Vector3.Distance(playerPosition, chunkCenter);

        // Get chunk bounds
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null) return; // Avoid errors if renderer is missing

        Bounds bounds = renderer.bounds;

        // Set Gizmo color based on distance
        Gizmos.color = distanceToPlayer <= distance ? Color.green : Color.red;

        // Draw a wire cube around the chunk
        //Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pos1, .25f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pos2, .25f);

        Gizmos.color = Color.red;
        try
        {
            foreach (var bound in worldCorners)
            {
                Gizmos.DrawWireSphere(bound, .5f);
            }
        }
        catch
        {

        }
    }

    private void Update()
    {
        Vector3 playerPosition = PlayerMovement.instance.transform.position;
        float distanceToPlayer = Vector2.Distance(new Vector2(playerPosition.x, playerPosition.z), new Vector2(chunkCenter.x, chunkCenter.z));


        if (distanceToPlayer <= distance)
        {
            rend.enabled = true;
            return;
        }

        if (LevelController.instance.s_frustumCulling)
        {
            bounds = rend.bounds;
            worldCorners = new Vector3[]
            {
                bounds.min,
                bounds.max,
                new Vector3(bounds.min.x, PlayerMovement.instance.transform.position.y, bounds.min.z),
                new Vector3(bounds.max.x, PlayerMovement.instance.transform.position.y, bounds.min.z),
                new Vector3(bounds.max.x, PlayerMovement.instance.transform.position.y, bounds.max.z),
                new Vector3(bounds.min.x, PlayerMovement.instance.transform.position.y, bounds.max.z)
            };

            bool isVisible = false;
            foreach (Vector3 corner in worldCorners)
            {
                Vector3 viewportPos = Camera.main.WorldToViewportPoint(corner);
                if (viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1)
                {
                    isVisible = true;
                    break;
                }
            }

            rend.enabled = isVisible;
        }
        else
        {
            rend.enabled = true;
        }
    }

    public void ProcessRayCast(RaycastHit hit, bool isPlacing)
    {
        if (!LevelController.instance.t_worldsave.modifiedChunks.ContainsKey(_chunkPos))
        {
            LevelController.instance.t_worldsave.modifiedChunks[_chunkPos] = new Dictionary<string, short>();
        }

        Vector3 hitPosition = hit.point - hit.normal * 0.1f; // Slight offset to prevent floating point issues
        Vector3 blockPos = new Vector3(Mathf.Floor(hitPosition.x), Mathf.Floor(hitPosition.y), Mathf.Floor(hitPosition.z));

        // Find the closest existing cube position
        if (cubes.Count > 0)
        {
            blockPos = cubes.Keys.OrderBy(pos => Vector3.Distance(pos, hitPosition)).First();
        }

        pos1 = hitPosition;
        pos2 = blockPos;

        if (!isPlacing)
        {
            // Remove the block
            if (cubes.ContainsKey(blockPos))
            {
                if (cubes[blockPos] == 9)
                {
                    return;
                }

                string pos = Helper.ConvertVector3ToString(blockPos);
                LevelController.instance.t_worldsave.modifiedChunks[_chunkPos][pos] = -1;
                cubes.Remove(blockPos);
                UpdateChunkMesh();
            }
        }
        else
        {
            // Place a new block at the adjacent position
            Vector3 newBlockPos = blockPos + hit.normal;
            if (!cubes.ContainsKey(newBlockPos))
            {
                cubes[newBlockPos] = PlayerMovement.instance.mouseLook.selectedCube;
                string pos = Helper.ConvertVector3ToString(newBlockPos);
                LevelController.instance.t_worldsave.modifiedChunks[_chunkPos][pos] = PlayerMovement.instance.mouseLook.selectedCube;
                UpdateChunkMesh();
            }
        }
    }


    private void UpdateChunkMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        Dictionary<Vector3, byte> lightLevels = new Dictionary<Vector3, byte>();
        List<Color> colors = new List<Color>();

        List<(Vector3, Quaternion, int, Vector2, Vector2)> faceData = new List<(Vector3, Quaternion, int, Vector2, Vector2)>();

        foreach (KeyValuePair<Vector3, short> cube in cubes)
        {
            byte lightLevel = BlocksManager.Instance.GetLightLevel((int)cube.Value);
            WorldGen.instance.CheckFaces(cube.Key, faceData, cubes, cube.Value, lightLevel, lightLevels, colors);
        }

        (Vector3[], int[], Vector2[]) meshData = WorldGen.instance.GenerateChunkCollider(faceData);

        StagedChunk chunk = new StagedChunk();
        chunk.lightLevels = lightLevels;
        chunk.vertices = meshData.Item1;
        chunk.triangles = meshData.Item2;
        chunk.uvs = meshData.Item3;
        chunk.chunkPosition = _chunk;
        chunk.cubePositions = cubes;
        chunk.colors = colors.ToArray();

        WorldGen.instance.loadedChunks.Remove(_chunk);
        WorldGen.instance.stagedChunks.Enqueue(chunk);
    }
}
