using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;

public class World : MonoBehaviour
{
    public TMP_Text worldName;
    public RawImage previewImage;
    private string _worldname;
    private WorldSave save;

    public void Init(string name)
    {
        _worldname = name;
        string fileName = $"\\{_worldname}";
        string path = Application.persistentDataPath + fileName;
        save = JsonConvert.DeserializeObject<WorldSave>(File.ReadAllText(path));

        Texture2D tex = WorldSave.ByteStringToTexture2D(save.image);
        tex.filterMode = FilterMode.Point;
        previewImage.texture = tex;
        worldName.text = save.worldName;
    }

    public void Load()
    {
        LevelController.instance.LoadWorld(_worldname.Split('.')[0]);
    }
}
