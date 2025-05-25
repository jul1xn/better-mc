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
        string fileName = $"\\saves\\{_worldname}";
        string path = Application.persistentDataPath + fileName;
        save = JsonConvert.DeserializeObject<WorldSave>(File.ReadAllText(path));
        worldName.text = save.worldName;

        Texture2D tex = Helper.ByteStringToTexture2D(save.image);
        tex.filterMode = FilterMode.Point;
        previewImage.texture = tex;
    }

    public void Load()
    {
        LevelController.instance.LoadWorld(_worldname.Split('.')[0]);
    }
}
