using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BlocksManager : MonoBehaviour
{
    public string blockPath;
    public string biomePath;
    public float biomeScale = 0.01f; // Controls biome size — higher scale = larger biomes
    public List<Block> allBlocks = new List<Block>();
    public List<Block> allOres = new List<Block>();
    public List<Biome> allBiomes = new List<Biome>();
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
        RefreshDB();
    }

    public void RefreshDB()
    {
        allBlocks.Clear();
        allBlocks = Resources.LoadAll<Block>(blockPath).ToList();

        allOres.Clear();
        allOres = allBlocks.Where(x => x.isOre).ToList();

        allBiomes.Clear();
        allBiomes = Resources.LoadAll<Biome>(biomePath).ToList();

        DataPackManager.Instance.ReloadAllPacks();
    }

    public short GetBlockByName(string name)
    {
        for (int i = 0; i < allBlocks.Count; i++)
        {
            Block b = allBlocks[i];
            if (b.blockName == name)
            {
                return (short)i;
            }
        }

        return (short)-1;
    }

    public Biome GetBiomeByName(string name)
    {
        foreach(Biome b in allBiomes)
        {
            if (name.ToLower() == b.biomeName.ToLower())
            {
                return b;
            }
        }

        return null;
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
        // Scale position down to stretch the noise
        float noiseX = position.x * biomeScale + seed;
        float noiseY = position.y * biomeScale + seed;

        float noiseValue = Mathf.PerlinNoise(noiseX, noiseY);

        // Weighted selection
        float totalWeight = allBiomes.Sum(b => b.commonness);
        float target = noiseValue * totalWeight;
        float cumulative = 0f;

        foreach (Biome biome in allBiomes)
        {
            cumulative += biome.commonness;
            if (target <= cumulative)
            {
                return biome;
            }
        }

        return allBiomes.Last();
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
