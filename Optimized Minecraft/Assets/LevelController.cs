using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static LevelController instance;
    public int w_worldType;
    public bool w_infiniteWorld;
    public float w_chunkSize;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        w_infiniteWorld = bool.Parse(PlayerPrefs.GetString("w_infiniteWorld", "false"));
        w_chunkSize = PlayerPrefs.GetFloat("w_chunkSize", 8);
        w_worldType = PlayerPrefs.GetInt("w_worldType", 0);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetString("w_infiniteWorld", w_infiniteWorld.ToString());
        PlayerPrefs.SetFloat("w_chunkSize", w_chunkSize);
        PlayerPrefs.SetInt("w_worldType", w_worldType);
    }
}
