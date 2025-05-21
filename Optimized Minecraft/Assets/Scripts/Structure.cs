using UnityEngine;

[CreateAssetMenu(fileName = "New Structure", menuName = "Minecraft/Structure")]
public class Structure : ScriptableObject
{
    public int commonness;
    public StructureBlock[] blocks;
}

[System.Serializable]
public struct StructureBlock
{
    public Vector3 relativePosition;
    public short blockId;
}