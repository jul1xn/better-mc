using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    public static PlayerUI instance;
    public bool chatOpen;
    public bool inventoryOpen;
    public bool inUI;
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
        inventoryUI.SetActive(false);
        chatInputField.gameObject.SetActive(false);
        inUI = false;
        debugMenu = false;

        sprites = BlocksManager.Instance.allBlocks;
        for (int i = 0; i < sprites.Length; i++)
        {
            Sprite tSprite = sprites[i].uiSprite;
            GameObject obj = Instantiate(uiPrefab, uiParent);
            obj.GetComponent<Image>().sprite = tSprite;

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
        if (debugMenu)
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
        if (Input.GetKeyDown(KeyCode.F3))
        {
            debugMenu = !debugMenu;
        }

        if (Input.GetKeyDown(KeyCode.T) && !chatOpen && !inventoryOpen)
        {
            chatOpen = true;
            inUI = true;
            PlayerMovement.instance.mouseLook.UnLockMouse();
            chatInputField.text = "";
            chatInputField.gameObject.SetActive(true);
            chatInputField.Select();
        }
        if (Input.GetKeyDown(KeyCode.E) && !inventoryOpen && !chatOpen)
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

            if (msgs[0] == "biome")
            {
                Biome targetBiome = BlocksManager.Instance.GetBiomeByName(msgs[1]);
                Vector2 targetPos = new Vector2(PlayerMovement.instance.transform.position.x, PlayerMovement.instance.transform.position.z);

                Biome foundBiome = null;
                int maxRadius = 10000;
                float step = 1f;

                bool biomeFound = false;

                for (int radius = 0; radius <= maxRadius && !biomeFound; radius++)
                {
                    for (float angle = 0; angle < 360f; angle += 10f)
                    {
                        float rad = angle * Mathf.Deg2Rad;
                        float x = targetPos.x + Mathf.Cos(rad) * radius * step;
                        float z = targetPos.y + Mathf.Sin(rad) * radius * step;
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
}
