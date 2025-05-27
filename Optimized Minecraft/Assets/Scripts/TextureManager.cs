using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
    public static TextureManager instance;

    public Dictionary<string, Vector2> textures = new Dictionary<string, Vector2>();
    public Texture2D textureAtlas;
    private int textureSize = 16;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void GenerateTextureAtlasFromResources()
    {
        Texture2D[] loadedTextures = Resources.LoadAll<Texture2D>("Textures/Blocks");

        if (loadedTextures.Length == 0)
        {
            Debug.LogError("No textures found in Resources/Textures/Blocks");
            return;
        }

        int count = loadedTextures.Length;
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(count));
        int atlasSize = gridSize * textureSize;

        textureAtlas = new Texture2D(atlasSize, atlasSize);
        textureAtlas.filterMode = FilterMode.Point; // For pixel art

        for (int i = 0; i < count; i++)
        {
            int x = (i % gridSize) * textureSize;
            int y = (i / gridSize) * textureSize;

            Texture2D tex = loadedTextures[i];

            if (tex.width != textureSize || tex.height != textureSize)
            {
                Debug.LogWarning($"{tex.name} is not {textureSize}x{textureSize}, scaling may be incorrect.");
            }

            Color[] pixels = tex.GetPixels(0, 0, textureSize, textureSize);
            textureAtlas.SetPixels(x, y, textureSize, textureSize, pixels);

            float u = (float)x / atlasSize;
            float v = (float)y / atlasSize;

            textures[tex.name] = new Vector2(u, v);
        }

        textureAtlas.Apply();
        textureAtlas.alphaIsTransparency = true;
        textureAtlas.filterMode = FilterMode.Point;
    }

    public Vector2 GetTextureUV(string textureName)
    {
        if (textures.TryGetValue(textureName, out Vector2 uv))
        {
            return uv;
        }

        Debug.LogWarning($"Texture name '{textureName}' not found in the atlas!");
        return Vector2.zero;
    }

    public void DumpAtlas()
    {
        if (!Directory.Exists(Application.persistentDataPath + "debug/"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "debug/");
        }
        byte[] pngData = textureAtlas.EncodeToPNG();
        string path = Application.persistentDataPath + "debug/atlas.png";
        File.WriteAllBytes(path, pngData);
        
        foreach(var kvp in textures)
        {
            File.AppendAllText(Application.persistentDataPath + "debug/atlas_index.txt", $"{kvp.Key}: {kvp.Value}");
        }
    }
}
