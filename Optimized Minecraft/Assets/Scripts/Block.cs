using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "New Block", menuName = "Minecraft/Block")]
public class Block : ScriptableObject
{
    public string blockName;
    public Sprite uiSprite;
    public TextureType texType;
    public bool isTransparent;
    public bool isFoiliage;
    [Space]
    public TargetTool targetTool;
    public float baseMiningSpeed = 1;
    [Space]
    public byte lightLevel;
    [Space]
    public bool isOre;
    public int oreVeinSize;
    public float oreRarity;

    [Space]

    [Tooltip("Used in: SingleTex, TopBottomAndSides, AllDifferent")]
    public string frontMainTexture;

    [Tooltip("Used in: AllDifferent")]
    public string backTexture;

    [Tooltip("Used in: AllDifferent")]
    public string leftTexture;

    [Tooltip("Used in: AllDifferent")]
    public string rightTexture;

    [Tooltip("Used in: TopBottomAndSides, AllDifferent")]
    public string topTexture;

    [Tooltip("Used in: TopBottomAndSides, AllDifferent")]
    public string bottomTexture;
}

public enum TextureType { SingleTex, TopBottomAndSides, AllDifferent }
public enum TargetTool { Shovel, Pickaxe, Axe, Hoe, Sword, Shears }