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

    Vector3 pos1;
    Vector3 pos2;

    private void Start()
    {
        distance = WorldGen.instance.chunkDistance;
        rend = GetComponent<MeshRenderer>();
        // Calculate the chunk center manually
        float chunkSize = WorldGen.instance.chunkSize;
        string[] lines = gameObject.name.Split(";");
        Vector3 actualPos = new Vector3(int.Parse(lines[0]), transform.position.y, int.Parse(lines[1]));
        chunkCenter = actualPos + new Vector3(chunkSize / 2f, chunkSize / 2f, chunkSize / 2f);
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !WorldGen.instance.debugDraw) return; // Avoid running in edit mode

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
        Gizmos.DrawWireCube(bounds.center, bounds.size);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pos1, .25f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(pos2, .25f);
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

        Bounds bounds = GetComponent<Renderer>().bounds;

        Vector3[] worldCorners = new Vector3[]
        {
            bounds.min,
            bounds.max,
            new Vector3(bounds.min.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z),
            new Vector3(bounds.max.x, bounds.min.y, bounds.max.z),
            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z)
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

        if (WorldGen.instance.type == WorldType.Amplified || !LevelController.instance.s_frustumCulling)
        {
            rend.enabled = true;
        }
        else
        {
            rend.enabled = isVisible;
        }
    }

    public void ProcessRayCast(RaycastHit hit, bool isPlacing)
    {
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
                string pos = WorldSave.ConvertVectorToString(blockPos);
                LevelController.instance.t_worldsave.modifiedBlocks[pos] = -1;
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
                string pos = WorldSave.ConvertVectorToString(newBlockPos);
                LevelController.instance.t_worldsave.modifiedBlocks[pos] = PlayerMovement.instance.mouseLook.selectedCube;
                UpdateChunkMesh();
            }
        }
    }




    private void UpdateChunkMesh()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        HashSet<Vector3> cubeSet = new HashSet<Vector3>(cubes.Keys);
        List<(Vector3, Quaternion, int, Vector2, Vector2)> faceData = new List<(Vector3, Quaternion, int, Vector2, Vector2)>();

        foreach (KeyValuePair<Vector3, short> cube in cubes)
        {
            WorldGen.instance.CheckFaces(cube.Key, faceData, cubeSet, cube.Value); // Use a default block ID (e.g., 34 for dirt)
        }

        (Vector3[], int[], Vector2[]) meshData = WorldGen.instance.GenerateChunkCollider(faceData);

        Mesh mesh = new Mesh();
        mesh.vertices = meshData.Item1;
        mesh.triangles = meshData.Item2;
        mesh.uv = meshData.Item3;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }

}
