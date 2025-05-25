using UnityEngine;

[CreateAssetMenu(fileName = "New Feature", menuName = "Minecraft/Feature")]
public class Feature : ScriptableObject
{
    public FeatureBlock[] blocks;
}

[System.Serializable]
public struct FeatureBlock
{
    public Vector3 relativePosition;
    public short blockId;
}