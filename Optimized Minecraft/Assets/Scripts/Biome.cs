using UnityEngine;

[CreateAssetMenu(fileName = "New Biome", menuName = "Minecraft/Biome")]
public class Biome : ScriptableObject
{
    public string biomeName;
    [Range(0f, 10f)]
    public float commonness = 1f;
    public Color foiliageColor;
    public short topBlockId = 2;
    public short middleBlockId = 1;
    public short bottomBlockId = 0;
    public SpawnableFeature[] spawnAbleFeatures;
}

[System.Serializable]
public struct SpawnableFeature
{
    public Feature feature;
    public int commonness;
}
