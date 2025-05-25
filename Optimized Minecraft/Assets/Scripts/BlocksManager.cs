using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BlocksManager : MonoBehaviour
{
    public string blockPath;
    public string structurePath;
    public string biomePath;
    public Block[] allBlocks;
    public Block[] allOres;
    public Structure[] allStructures;
    public Biome[] allBiomes;
    public static BlocksManager Instance;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void Start()
    {
        allBlocks = Resources.LoadAll<Block>(blockPath);
        allOres = allBlocks.Where(x => x.isOre).ToArray();
        allStructures = Resources.LoadAll<Structure>(structurePath);
        allBiomes = Resources.LoadAll<Biome>(biomePath);
    }

    public short GetBlockByName(string name)
    {
        for (int i = 0; i < allBlocks.Length; i++)
        {
            Block b = allBlocks[i];
            if (b.blockName == name)
            {
                return (short)i;
            }
        }

        return (short)-1;
    }

    public byte GetLightLevel(int blockId)
    {
        return allBlocks[blockId].lightLevel;
    }

    public bool IsTransparent(int blockId)
    {
        return allBlocks[blockId].isTransparent;
    }

    public bool IsFoiliage(int blockId)
    {
        return allBlocks[blockId].isFoiliage;
    }

    public Biome GetBiomeAtPos(Vector2 position, float seed)
    {
        // Create a Perlin noise coordinate based on position and seed to get consistent but varied values
        float noiseValue = Mathf.PerlinNoise(position.x + seed, position.y + seed);

        // Map noise value (0 to 1) to an index in allBiomes
        int biomeIndex = Mathf.FloorToInt(noiseValue * allBiomes.Length);

        // Clamp in case noiseValue == 1
        biomeIndex = Mathf.Clamp(biomeIndex, 0, allBiomes.Length - 1);

        return allBiomes[biomeIndex];
    }


    public static short GetTextureIndexForFace(Block block, string face)
    {
        switch (block.texType)
        {
            case TextureType.SingleTex:
                return block.frontMainTexture;

            case TextureType.TopBottomAndSides:
                if (face == "top") return block.topTexture;
                if (face == "bottom") return block.bottomTexture;
                return block.frontMainTexture;

            case TextureType.AllDifferent:
                return face switch
                {
                    "front" => block.frontMainTexture,
                    "back" => block.backTexture,
                    "left" => block.leftTexture,
                    "right" => block.rightTexture,
                    "top" => block.topTexture,
                    "bottom" => block.bottomTexture,
                    _ => block.frontMainTexture
                };
        }

        return block.frontMainTexture; // fallback
    }

}
