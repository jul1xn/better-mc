using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
    public static TextureManager instance;
    public Dictionary<string, Vector2> textures = new Dictionary<string, Vector2>();
    public Texture2D textureAtlas;
    private int textureSize = 16;
    private List<Texture2D> texs = new List<Texture2D>();
    private int atlasSize;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning(gameObject.name);
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    public void AddNewTexture(Texture2D tex)
    {
        texs.Add(tex);
    }

    public void GenerateTextureAtlasFromResources()
    {
        texs.AddRange(Resources.LoadAll<Texture2D>("Textures/Blocks"));

        if (texs.Count == 0)
        {
            Debug.LogError("No textures found in Resources/Textures/Blocks");
            return;
        }

        int count = texs.Count;
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(count));
        atlasSize = gridSize * textureSize;

        textureAtlas = new Texture2D(atlasSize, atlasSize);
        textureAtlas.filterMode = FilterMode.Point; // For pixel art

        for (int i = 0; i < count; i++)
        {
            int x = (i % gridSize) * textureSize;
            int y = (i / gridSize) * textureSize;

            Texture2D tex = texs[i];

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
        Debug.Log("Created texture atlas");

        texs = new List<Texture2D>();
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
        if (textureAtlas == null)
        {
            Debug.LogError("Texture atlas not generated yet.");
            return;
        }

        string debugDir = Path.Combine(Application.persistentDataPath, "debug");

        if (!Directory.Exists(debugDir))
        {
            Directory.CreateDirectory(debugDir);
        }

        string atlasPath = Path.Combine(debugDir, "atlas.png");
        byte[] pngData = textureAtlas.EncodeToPNG();
        File.WriteAllBytes(atlasPath, pngData);
        Debug.Log("Saved atlas to " + atlasPath);

        string indexPath = Path.Combine(debugDir, "atlas_index.txt");
        using (StreamWriter sw = new StreamWriter(indexPath, false))
        {
            foreach (var kvp in textures)
            {
                sw.WriteLine($"{kvp.Key}: {kvp.Value}");
            }
        }

        Debug.Log("Saved atlas index to " + indexPath);
    }

}
