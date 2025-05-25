using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public static MainMenuUI Instance;
    public AudioSource clickAudio;
    [Space]
    public GameObject mainUI;
    public GameObject singleUI;
    public GameObject optionsUI;
    public GameObject worldUI;
    public GameObject multiplayerUI;
    public GameObject createWorldUI;
    public GameObject dataPackUI;
    [Space]
    public GameObject mpUsernameUI;
    public GameObject mpConnectUI;
    public TMP_InputField mpUsernameInputfield;
    [Space]
    public Toggle infiniteToggle;
    public CanvasGroup worldSizeCanvas;
    public Slider worldSize;
    public TMP_Dropdown worldType;
    [Space]
    public TMP_InputField worldName;
    public GameObject worldPrefab;
    public Transform worldParent;
    [Space]
    public GameObject dpPrefab;
    public Transform dpParent;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        mainUI.SetActive(true);
        singleUI.SetActive(false);
        optionsUI.SetActive(false);
        worldUI.SetActive(false);
        multiplayerUI.SetActive(false);
        createWorldUI.SetActive(false);
        dataPackUI.SetActive(false);

        infiniteToggle.isOn = LevelController.instance.w_infiniteWorld;
        worldSize.value = LevelController.instance.w_chunkSize;
        worldType.value = LevelController.instance.w_worldType;
        worldType.RefreshShownValue();

        worldSize.interactable = !infiniteToggle.isOn;
        if (!infiniteToggle.isOn)
        {
            worldSizeCanvas.alpha = 1f;
        }
        else
        {
            worldSizeCanvas.alpha = 0.5f;
        }

        LoadWorlds();

        foreach (Button btn in Resources.FindObjectsOfTypeAll<Button>())
        {
            btn.onClick.AddListener(() =>
            {
                clickAudio.Play();
            });
        }
    }

    public void LoadWorlds()
    {
        DirectoryInfo d = new DirectoryInfo(Application.persistentDataPath + "\\saves\\");
        foreach(Transform t in worldParent)
        {
            Destroy(t.gameObject);
        }

        foreach(var file in d.GetFiles("*.save"))
        {
            GameObject obj = Instantiate(worldPrefab, worldParent);
            obj.GetComponent<World>().Init(file.Name);
        }
    }

    public void LoadDataPacks()
    {
        foreach (Transform t in dpParent)
        {
            Destroy(t.gameObject);
        }

        foreach (DatapackData dp in DataPackManager.Instance.GetDatapacks())
        {
            GameObject obj = Instantiate(dpPrefab, dpParent);
            obj.GetComponent<DataPackUI>().Init(dp);
        }
    }

    public void RefreshDataPacks()
    {
        BlocksManager.Instance.RefreshDB();
        LoadDataPacks();
    }

    public void DownloadExamplePack()
    {
        float chance = Random.value;

        if (chance < 0.05f)
        {
            Application.OpenURL("https://youtu.be/dQw4w9WgXcQ");
        }
        else
        {
            Application.OpenURL("https://prowser.nl/downloads/bmc_example_datapack.zip");
        }
    }

    public void OpenDPFolder()
    {
        System.Diagnostics.Process.Start(Application.persistentDataPath + DataPackManager.dpPath);
    }

    public void OpenMultiplayerMenu()
    {
        mainUI.SetActive(false);
        multiplayerUI.SetActive(true);
        if (PlayerPrefs.GetString("mpUsername", "") == "")
        {
            mpUsernameUI.SetActive(true);
            mpConnectUI.SetActive(false);
        }
        else
        {
            mpUsernameUI.SetActive(false);
            mpConnectUI.SetActive(true);
        }
    }

    public void MPConfirmUsername()
    {
        string username = mpUsernameInputfield.text.Replace(' ', '_');
        if (!string.IsNullOrEmpty(username))
        {
            PlayerPrefs.SetString("mpUsername", username);
        }

        OpenMultiplayerMenu();
    }

    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void CreateWorld()
    {
        if (!string.IsNullOrWhiteSpace(worldName.text))
        {
            LevelController.instance.LoadWorld(worldName.text);
        }
    }

    public void SetWorldType(int value)
    {
        LevelController.instance.w_worldType = value;
    }

    public void SetInfiniteToggle(bool toggled)
    {
        LevelController.instance.w_infiniteWorld = toggled;
        worldSize.interactable = !toggled;
        if (!toggled)
        {
            worldSizeCanvas.alpha = 1f;
        }
        else
        {
            worldSizeCanvas.alpha = 0.5f;
        }
    }

    public void SetWorldSize(float size)
    {
        LevelController.instance.w_chunkSize = size;
    }
}
