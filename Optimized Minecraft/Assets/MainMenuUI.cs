using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public AudioSource clickAudio;
    [Space]
    public GameObject mainUI;
    public GameObject singleUI;
    public GameObject multiUI;
    public GameObject optionsUI;
    public GameObject worldUI;
    [Space]
    public Toggle infiniteToggle;
    public CanvasGroup worldSizeCanvas;
    public Slider worldSize;
    public TMP_Dropdown worldType;
    [Space]
    public TMP_Text world0Text;
    public TMP_Text world1Text;
    public TMP_Text world2Text;
    public TMP_Text world3Text;
    public TMP_Text world4Text;

    private void Start()
    {
        mainUI.SetActive(true);
        singleUI.SetActive(false);
        multiUI.SetActive(false);
        optionsUI.SetActive(false);
        worldUI.SetActive(false);

        world0Text.text = "World 1 " + LevelController.GetMenuFileSize(0);
        world1Text.text = "World 2 " + LevelController.GetMenuFileSize(1);
        world2Text.text = "World 3 " + LevelController.GetMenuFileSize(2);
        world3Text.text = "World 4 " + LevelController.GetMenuFileSize(3);
        world4Text.text = "World 5 " + LevelController.GetMenuFileSize(4);

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

        foreach (Button btn in Resources.FindObjectsOfTypeAll<Button>())
        {
            btn.onClick.AddListener(() =>
            {
                clickAudio.Play();
            });
        }
    }

    public void ExitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    public void LoadWorld(int index)
    {
        LevelController.instance.LoadWorld(index);
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
