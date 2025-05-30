using System.IO;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System;
using UnityEngine.SceneManagement;
using System.Reflection;

public class LevelController : MonoBehaviour
{
    public static LevelController instance;
    public int w_worldType;
    public bool w_infiniteWorld;
    public float w_chunkSize;
    [Space]
    public bool s_frustumCulling;
    public int s_rendDist;
    public float s_fov;
    public int s_quality;
    public int s_antia;
    public bool s_transparentculling;
    [Space]
    public string t_loadedWorldName;
    public WorldSave t_worldsave;

    public static Texture2D CaptureScreenshot64x64()
    {
        // Create a 64x64 RenderTexture
        RenderTexture rt = new RenderTexture(64, 64, 24);
        Texture2D screenshot = new Texture2D(64, 64, TextureFormat.RGB24, false);

        // Render the screen to the texture
        Camera.main.targetTexture = rt;
        Camera.main.Render();
        RenderTexture.active = rt;

        // Read the pixels into the Texture2D
        screenshot.ReadPixels(new Rect(0, 0, 64, 64), 0, 0);
        screenshot.Apply();

        // Clean up
        Camera.main.targetTexture = null;
        RenderTexture.active = null;
        UnityEngine.Object.Destroy(rt);

        return screenshot;
    }

    private void Awake()
    {
        instance = this;

        DontDestroyOnLoad(gameObject);

        w_infiniteWorld = bool.Parse(PlayerPrefs.GetString("w_infiniteWorld", "false"));
        s_frustumCulling = bool.Parse(PlayerPrefs.GetString("s_frustumCulling", "true"));
        w_chunkSize = PlayerPrefs.GetFloat("w_chunkSize", 8);
        s_fov = PlayerPrefs.GetFloat("s_fov", 60);
        w_worldType = PlayerPrefs.GetInt("w_worldType", 0);
        s_rendDist = PlayerPrefs.GetInt("s_rendDist", 8);
        s_quality = PlayerPrefs.GetInt("s_quality", 2);
        s_antia = PlayerPrefs.GetInt("s_antia", 3);
        s_transparentculling = bool.Parse(PlayerPrefs.GetString("s_transparentculling", "false"));

        Debug.Log(Application.persistentDataPath);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetString("w_infiniteWorld", w_infiniteWorld.ToString());
        PlayerPrefs.SetString("s_frustumCulling", s_frustumCulling.ToString());
        PlayerPrefs.SetString("s_transparentculling", s_transparentculling.ToString());
        PlayerPrefs.SetFloat("w_chunkSize", w_chunkSize);
        PlayerPrefs.SetFloat("s_fov", s_fov);
        PlayerPrefs.SetInt("w_worldType", w_worldType);
        PlayerPrefs.SetInt("s_rendDist", s_rendDist);
        PlayerPrefs.SetInt("s_quality", s_quality);
        PlayerPrefs.SetInt("s_antia", s_antia);

        if (SceneManager.GetActiveScene().buildIndex == 2)
        {
            SaveWorld();
        }
    }

    public void SaveAndQuit()
    {
        SaveWorld();
        SceneManager.LoadScene(1);
    }

    System.Random r = new System.Random();

    public void LoadWorld(string name)
    {
        t_loadedWorldName = Helper.ConvertToValidFilename(name);
        string fileName = $"\\saves\\{t_loadedWorldName}.save";
        string path = Application.persistentDataPath + fileName;

        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        if (File.Exists(path))
        {
            t_worldsave = JsonConvert.DeserializeObject<WorldSave>(File.ReadAllText(path));
        }
        else
        {
            t_worldsave = new WorldSave();
            t_worldsave.worldName = name;
            t_worldsave.worldType = 0;
            t_worldsave.seed = float.Parse(r.Next(0, 100000).ToString()) / 1000;
            t_worldsave.modifiedChunks = new Dictionary<string, Dictionary<string, short>>();

            float noiseValue = WorldGen.GetNoise(t_worldsave.seed, 0, 0);
            int height = Mathf.RoundToInt(noiseValue * WorldGen.maxHeight);

            Vector3 playerPosition = new Vector3(0, (height * 2) + 1, 0);
            t_worldsave.playerPosition = Helper.ConvertVector3ToString(playerPosition);
            t_worldsave.playerRotation = Helper.ConvertVector3ToString(Vector3.zero);
        }

        SceneManager.LoadScene(2);
    }

    public void SaveWorld()
    {
        string fileName = $"\\saves\\{t_loadedWorldName}.save";
        string path = Application.persistentDataPath + fileName;

        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        t_worldsave.playerPosition = Helper.ConvertVector3ToString(PlayerMovement.instance.transform.position);
        t_worldsave.playerRotation = Helper.ConvertVector3ToString(new Vector3(PlayerMovement.instance.mouseLook.transform.eulerAngles.x, PlayerMovement.instance.transform.eulerAngles.y, 0f));
        t_worldsave.image = Helper.Texture2DToByteString(CaptureScreenshot64x64());

        string data = JsonConvert.SerializeObject(t_worldsave);
        File.WriteAllText(path, data);
        Debug.Log($"World {t_loadedWorldName} at {path}");
    }
}

[Serializable]
public class WorldSave
{
    public string worldName;
    public string image;
    public float seed;
    public int worldType;
    public Dictionary<string, Dictionary<string, short>> modifiedChunks;
    public string playerPosition;
    public string playerRotation;
}