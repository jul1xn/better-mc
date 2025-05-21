using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlocksManager : MonoBehaviour
{
    public string blockPath;
    public Block[] allBlocks;
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
