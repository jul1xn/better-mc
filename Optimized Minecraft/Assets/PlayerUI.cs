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
    Sprite[] sprites;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        inventoryUI.SetActive(false);
        chatInputField.gameObject.SetActive(false);
        inUI = false;

        sprites = Resources.LoadAll<Sprite>(spritePath);
        for (int i = 0; i < sprites.Length; i++)
        {
            GameObject obj = Instantiate(uiPrefab, uiParent);
            obj.GetComponent<Image>().sprite = sprites[i];

            int index = i;

            obj.GetComponent<Button>().onClick.AddListener(() =>
            {
                PlayerMovement.instance.mouseLook.selectedCube = (short)(index+32);
                PlayerMovement.instance.mouseLook.LockMouse();
                inventoryUI.SetActive(false);
                inventoryOpen = false;
                inUI = false;
            });
        }
    }

    private void Update()
    {
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
        }
    }
}
