using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI instance;
    public GameObject mainUI;
    public GameObject pauseUI;
    [Space]
    public bool chatOpen;
    public bool inventoryOpen;
    public bool inUI;
    public bool gamePaused;
    public TMP_InputField chatInputField;
    public GameObject inventoryUI;
    public GameObject uiPrefab;
    public Transform uiParent;
    public string spritePath;
    Block[] sprites;
    bool debugMenu;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        mainUI.SetActive(true);
        pauseUI.SetActive(false);
        inventoryUI.SetActive(false);
        chatInputField.gameObject.SetActive(false);
        inUI = false;
        debugMenu = false;
        gamePaused = false;

        sprites = BlocksManager.Instance.allBlocks.ToArray();
        for (int i = 0; i < sprites.Length; i++)
        {
            GameObject obj = Instantiate(uiPrefab, uiParent);
            RawImage img = obj.GetComponent<RawImage>();
            img.material = WorldGen.instance.sharedMaterial;

            float uvSize = 16f / TextureManager.instance.textureAtlas.width; // size in normalized units
            Vector2 uvBase = TextureManager.instance.GetTextureUV(sprites[i].frontMainTexture); // Assuming a method to get this
            img.uvRect = new Rect(uvBase.x, uvBase.y, uvSize, uvSize);

            int index = i;
            obj.GetComponent<Button>().onClick.AddListener(() =>
            {
                PlayerMovement.instance.mouseLook.selectedCube = (short)index;
                PlayerMovement.instance.mouseLook.LockMouse();
                inventoryUI.SetActive(false);
                inventoryOpen = false;
                inUI = false;
            });
        }
    }

    private void OnGUI()
    {
        if (debugMenu && !gamePaused)
        {
            GUILayoutOption[] options = new GUILayoutOption[] {
                GUILayout.Height(20)
            };
            GUILayout.Label($"{Application.productName} ({Application.version}) - {Application.companyName}", options);
            GUILayout.Label($"{Mathf.Round(1 / Time.deltaTime)} FPS ({Time.deltaTime})", options);
            GUILayout.Label("", options);
            GUILayout.Label($"XYZ: {PlayerMovement.instance.transform.position.x} / {PlayerMovement.instance.transform.position.y} / {PlayerMovement.instance.transform.position.z}", options);
            GUILayout.Label($"Velocity: {PlayerMovement.instance.controller.velocity.magnitude}", options);
            GUILayout.Label("", options);
            GUILayout.Label($"Loaded chunks: {WorldGen.instance.loadedChunks.Count}", options);
            GUILayout.Label($"Child count: {WorldGen.instance.transform.childCount}", options);
            GUILayout.Label($"Staged chunks: {WorldGen.instance.stagedChunks.Count}", options);
            GUILayout.Label($"Active threads: {WorldGen.instance.activeThreads}", options);
            GUILayout.Label("", options);
            GUILayout.Label($"Chunk count: {WorldGen.instance.transform.childCount}", options);

            int count = 0;
            foreach(Transform child in WorldGen.instance.transform)
            {
                if (!child.GetComponent<MeshRenderer>().enabled)
                {
                    count++;
                }
            }

            GUILayout.Label($"Culled chunks: {count}", options);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gamePaused = !gamePaused;
            mainUI.SetActive(!mainUI.activeSelf);
            pauseUI.SetActive(!pauseUI.activeSelf);

            if (gamePaused)
            {
                PlayerMovement.instance.mouseLook.UnLockMouse();
            }
            else
            {
                PlayerMovement.instance.mouseLook.LockMouse();
            }
        }

        if (Input.GetKeyDown(KeyCode.F3) && !gamePaused)
        {
            debugMenu = !debugMenu;
        }

        if (Input.GetKeyDown(KeyCode.T) && !chatOpen && !inventoryOpen && !gamePaused)
        {
            chatOpen = true;
            inUI = true;
            PlayerMovement.instance.mouseLook.UnLockMouse();
            chatInputField.text = "";
            chatInputField.gameObject.SetActive(true);
            chatInputField.Select();
        }
        if (Input.GetKeyDown(KeyCode.E) && !inventoryOpen && !chatOpen && !gamePaused)
        {
            inventoryOpen = true;
            inUI = true;
            PlayerMovement.instance.mouseLook.UnLockMouse();
            inventoryUI.SetActive(true);
        }
    }

    public void ChatEnd(string msg)
    {
        chatOpen = false;
        inUI = false;
        PlayerMovement.instance.mouseLook.LockMouse();
        chatInputField.gameObject.SetActive(false);
        
        if (msg.StartsWith("/"))
        {
            msg = msg.Replace("/", "");
            string[] msgs = msg.Split(" ");
            if (msgs[0] == "tp")
            {
                Vector3 targetPos = new Vector3(int.Parse(msgs[1]), int.Parse(msgs[2]), int.Parse(msgs[3]));
                PlayerMovement.instance.TeleportToPosition(targetPos);
            }

            if (msgs[0] == "summon")
            {
                EntityManager.instance.SpawnEntityAt(PlayerMovement.instance.transform.position, EntityType.Cow);
            }

            if (msgs[0] == "debugmode")
            {
                WorldGen.instance.debugDraw = !WorldGen.instance.debugDraw;
            }

            if (msgs[0] == "disablegroundcheck")
            {
                PlayerMovement.instance.groundGenerated = true;
            }

            if (msgs[0] == "fly")
            {
                PlayerMovement.instance.ToggleFly();
            }

            if (msgs[0] == "debug")
            {
                if (msgs[1] == "dumpatlas")
                {
                    TextureManager.instance.DumpAtlas();
                }
            }

            if (msgs[0] == "biome")
            {
                Biome targetBiome = BlocksManager.Instance.GetBiomeByName(msgs[1]);
                Vector2 targetPos = new Vector2(Mathf.Round(PlayerMovement.instance.transform.position.x), Mathf.Round(PlayerMovement.instance.transform.position.z));

                Biome foundBiome = null;
                int maxRadius = 10000;
                float step = 1f;

                bool biomeFound = false;

                for (int radius = 0; radius <= maxRadius && !biomeFound; radius++)
                {
                    for (float angle = 0; angle < 360f; angle += 10f)
                    {
                        float rad = angle * Mathf.Deg2Rad;
                        float x = Mathf.Round(targetPos.x + Mathf.Cos(rad) * radius * step);
                        float z = Mathf.Round(targetPos.y + Mathf.Sin(rad) * radius * step);
                        Vector2 checkPos = new Vector2(x, z);

                        foundBiome = BlocksManager.Instance.GetBiomeAtPos(checkPos, WorldGen.instance.seed);
                        if (foundBiome != null && foundBiome.biomeName == targetBiome.biomeName)
                        {
                            Debug.Log($"Biome '{targetBiome.biomeName}' found at: {checkPos}");
                            biomeFound = true;
                            break;
                        }
                    }
                }

                if (!biomeFound)
                {
                    Debug.LogWarning($"Biome '{targetBiome.biomeName}' not found within radius {maxRadius}");
                }
            }

        }
    }

    public void SaveAndQuit()
    {
        LevelController.instance.SaveAndQuit();
    }

    public void Resume()
    {
        gamePaused = !gamePaused;
        mainUI.SetActive(!mainUI.activeSelf);
        pauseUI.SetActive(!pauseUI.activeSelf);

        if (gamePaused)
        {
            PlayerMovement.instance.mouseLook.UnLockMouse();
        }
        else
        {
            PlayerMovement.instance.mouseLook.LockMouse();
        }
    }
}
