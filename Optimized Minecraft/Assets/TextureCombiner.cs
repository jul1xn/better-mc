using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

public class TextureCombiner : MonoBehaviour
{
    public string resourcesFolder = "Textures/Blocks"; // Inside Resources/

    [ContextMenu("Combine Textures")]
    void CombineTextures()
    {
        Texture2D[] textures = Resources.LoadAll<Texture2D>(resourcesFolder);

        if (textures.Length == 0)
        {
            Debug.LogError("No textures found in Resources/" + resourcesFolder);
            return;
        }

        int texWidth = textures[0].width;
        int texHeight = textures[0].height;

        // Grid size
        int gridSize = Mathf.CeilToInt(Mathf.Sqrt(textures.Length));
        int combinedWidth = gridSize * texWidth;
        int combinedHeight = gridSize * texHeight;

        Texture2D combined = new Texture2D(combinedWidth, combinedHeight);

        for (int i = 0; i < textures.Length; i++)
        {
            int x = (i % gridSize) * texWidth;
            int y = (i / gridSize) * texHeight;

            Color[] pixels = textures[i].GetPixels();
            combined.SetPixels(x, y, texWidth, texHeight, pixels);
        }

        combined.Apply();

#if UNITY_EDITOR
        // Save the texture as PNG (Editor only)
        byte[] pngData = combined.EncodeToPNG();
        string path = "Assets/CombinedTexture.png";
        File.WriteAllBytes(path, pngData);
        AssetDatabase.Refresh();
        Debug.Log("Combined texture saved to: " + path);
#else
        Debug.Log("Texture combined at runtime (not saved).");
#endif
    }
}
