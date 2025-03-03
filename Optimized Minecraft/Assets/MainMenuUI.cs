using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    public Toggle infiniteToggle;
    public CanvasGroup worldSizeCanvas;
    public Slider worldSize;
    public TMP_Dropdown worldType;

    private void Start()
    {
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
    }

    public void LoadWorld()
    {
        SceneManager.LoadSceneAsync(1);
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
