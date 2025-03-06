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
            t_worldsave.modifiedBlocks = new Dictionary<string, short>();

            Vector3 playerPosition = new Vector3(0, WorldGen.GetNoise(t_worldsave.seed, 0, 0), 0);
            t_worldsave.playerPosition = WorldSave.ConvertVectorToString(playerPosition);
        }

        SceneManager.LoadScene(1);
    }

    public void SaveWorld()
    {
        string fileName = $"\\world_{t_loadedWorldIndex}.save";
        string path = Application.persistentDataPath + fileName;

        t_worldsave.playerPosition = WorldSave.ConvertVectorToString(PlayerMovement.instance.transform.position);

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

    public short IsBlockModified(Vector3 position)
    {
        string pos = WorldSave.ConvertVectorToString(position);
        if (t_worldsave.modifiedBlocks.ContainsKey(pos))
        {
            return t_worldsave.modifiedBlocks[pos];
        }

        return -2;
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
    public Dictionary<string, short> modifiedBlocks;
    public string playerPosition;

    public static string ConvertVectorToString(Vector3 position)
    {
        return $"{position.x};{position.y};{position.z}";
    }

    public static Vector3 ConvertStringToVector(string value)
    {
        string[] lines = value.Split(';');
        return new Vector3(float.Parse(lines[0]), float.Parse(lines[1]), float.Parse(lines[2]));
    }
}