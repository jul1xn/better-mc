using UnityEngine;

[CreateAssetMenu(fileName = "New Block", menuName = "Minecraft/Block")]
public class Block : ScriptableObject
{
    public string blockName;
    public Sprite uiSprite;
    public TextureType texType;
    [Space]
    public byte lightLevel;
    [Space]
    public bool isOre;
    public int oreVeinSize;
    public float oreRarity;

    [Space]

    [Tooltip("Used in: SingleTex, TopBottomAndSides, AllDifferent")]
    public short frontMainTexture;

    [Tooltip("Used in: AllDifferent")]
    public short backTexture;

    [Tooltip("Used in: AllDifferent")]
    public short leftTexture;

    [Tooltip("Used in: AllDifferent")]
    public short rightTexture;

    [Tooltip("Used in: TopBottomAndSides, AllDifferent")]
    public short topTexture;

    [Tooltip("Used in: TopBottomAndSides, AllDifferent")]
    public short bottomTexture;
}

public enum TextureType { SingleTex, TopBottomAndSides, AllDifferent }