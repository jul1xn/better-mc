using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureManager : MonoBehaviour
{
    public static TextureManager instance;
    public Dictionary<short, Vector2> textures = new Dictionary<short, Vector2>();
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
        GenerateTextureAtlas();
    }

    private void GenerateTextureAtlas()
    {
        if (textureAtlas == null)
        {
            Debug.LogError("Texture Atlas is not assigned!");
            return;
        }

        short blockId = 0;
        int columns = textureAtlas.width / textureSize;
        int rows = (textureAtlas.height / textureSize) * 2;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                float u = (float)x / columns;
                float v = (float)(rows - y) / rows;


                textures[blockId] = new Vector2(u, v);
                blockId++;
            }
        }
    }

    public Vector2 GetTextureUV(short blockId)
    {
        if (textures.ContainsKey(blockId))
        {
            return textures[blockId];
        }
        else
        {
            Debug.LogWarning($"Block ID {blockId} not found in the texture atlas!");
            return Vector2.zero;
        }
    }
}
