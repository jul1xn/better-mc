using System.IO;
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
using Palmmedia.ReportGenerator.Core.Parser.Analysis;

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
    [Space]
    public int t_loadedWorldIndex;
    public WorldSave t_worldsave;

    private void Awake()
    {
        instance = this;
        t_loadedWorldIndex = 0;
        DontDestroyOnLoad(gameObject);

        w_infiniteWorld = bool.Parse(PlayerPrefs.GetString("w_infiniteWorld", "false"));
        s_frustumCulling = bool.Parse(PlayerPrefs.GetString("s_frustumCulling", "true"));
        w_chunkSize = PlayerPrefs.GetFloat("w_chunkSize", 8);
        s_fov = PlayerPrefs.GetFloat("s_fov", 60);
        w_worldType = PlayerPrefs.GetInt("w_worldType", 0);
        s_rendDist = PlayerPrefs.GetInt("s_rendDist", 8);
    }

    private void OnApplicationQuit()
    {
        PlayerPrefs.SetString("w_infiniteWorld", w_infiniteWorld.ToString());
        PlayerPrefs.SetString("s_frustumCulling", s_frustumCulling.ToString());
        PlayerPrefs.SetFloat("w_chunkSize", w_chunkSize);
        PlayerPrefs.SetFloat("s_fov", s_fov);
        PlayerPrefs.SetInt("w_worldType", w_worldType);
        PlayerPrefs.SetInt("s_rendDist", s_rendDist);

        if (SceneManager.GetActiveScene().buildIndex == 1)
        {
            SaveWorld();
        }
    }

    System.Random r = new System.Random();

    public void LoadWorld(int index)
    {
        t_loadedWorldIndex = index;
        string fileName = $"\\world_{index}.save";
        string path = Application.persistentDataPath + fileName;

        if (File.Exists(path))
        {
            t_worldsave = JsonConvert.DeserializeObject<WorldSave>(File.ReadAllText(path));
        }
        else
        {
            t_worldsave = new WorldSave();
            t_worldsave.worldType = 0;
            t_worldsave.seed = float.Parse(r.Next(0, 100000).ToString()) / 1000;
            t_worldsave.modifiedChunks = new Dictionary<string, Dictionary<string, short>>();

            float noiseValue = WorldGen.GetNoise(t_worldsave.seed, 0, 0);
            int height = Mathf.RoundToInt(noiseValue * WorldGen.maxHeight);

            Vector3 playerPosition = new Vector3(0, (height * 2) + 1, 0);
            t_worldsave.playerPosition = WorldSave.ConvertVector3ToString(playerPosition);
            t_worldsave.playerRotation = WorldSave.ConvertVector3ToString(Vector3.zero);
        }

        SceneManager.LoadScene(1);
    }

    public void SaveWorld()
    {
        string fileName = $"\\world_{t_loadedWorldIndex}.save";
        string path = Application.persistentDataPath + fileName;

        t_worldsave.playerPosition = WorldSave.ConvertVector3ToString(PlayerMovement.instance.transform.position);
        t_worldsave.playerRotation = WorldSave.ConvertVector3ToString(new Vector3(PlayerMovement.instance.mouseLook.transform.eulerAngles.x, PlayerMovement.instance.transform.eulerAngles.y, 0f));

        string data = JsonConvert.SerializeObject(t_worldsave);
        File.WriteAllText(path, data);
        Debug.Log($"World {t_loadedWorldIndex} at {path}");
    }

    public static string GetBoolText(bool value)
    {
        if (value)
        {
            return "ON";
        }
        else
        {
            return "OFF";
        }
    }

    public static string GetMenuFileSize(int index)
    {
        string fileName = $"\\world_{index}.save";
        string path = Application.persistentDataPath + fileName;
        if (File.Exists(path))
        {
            FileInfo fileInfo = new FileInfo(path);
            float fileSize = Mathf.Round(fileInfo.Length / 10000) / 100;
            return $"({fileSize} MB)";
        }
        else
        {
            return "(? MB)";
        }
    }
}

[Serializable]
public class WorldSave
{
    public float seed;
    public int worldType;
    public Dictionary<string, Dictionary<string, short>> modifiedChunks;
    public string playerPosition;
    public string playerRotation;

    public static string ConvertVector2ToString(Vector2 position)
    {
        return $"{position.x};{position.y}";
    }

    public static string ConvertVector3ToString(Vector3 position)
    {
        return $"{position.x};{position.y};{position.z}";
    }

    public static Vector3 ConvertStringToVector3(string value)
    {
        string[] lines = value.Split(';');
        return new Vector3(float.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]));
    }

    public static Vector2 ConvertStringToVector2(string value)
    {
        string[] lines = value.Split(';');
        return new Vector2(float.Parse(lines[0]), float.Parse(lines[1]));
    }
}