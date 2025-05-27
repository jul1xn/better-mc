using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.UIElements;
using System.Security.Cryptography;

public class DataPackManager : MonoBehaviour
{
    public static DataPackManager Instance;
    public static string dpPath = "\\datapacks\\";
    public List<string> disabledPacks = new List<string>();
    private static string ddpPath = "\\disabled_datapacks.json";

    private void Awake()
    {
        Instance = this;
        if (File.Exists(Application.persistentDataPath + ddpPath))
        {
            disabledPacks = JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Application.persistentDataPath + ddpPath));
        }
    }

    private void OnApplicationQuit()
    {
        File.WriteAllText(Application.persistentDataPath + ddpPath, JsonConvert.SerializeObject(disabledPacks));
    }

    public void ReloadAllPacks()
    {
        foreach (var dir in Directory.GetDirectories(Application.persistentDataPath + dpPath))
        {
            if (disabledPacks.Contains(Path.GetFileName(dir)))
            {
                return;
            }

            string textures = dir + "\\textures\\";
            string biomes = dir + "\\biomes\\";
            string features = dir + "\\features\\";

            if (Directory.Exists(textures))
            {
                foreach(var file in Directory.GetFiles(textures).Where(t => t.EndsWith(".png")))
                {
                    Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                    byte[] bytes = File.ReadAllBytes(file);
                    tex.LoadImage(bytes);
                    tex.Apply();
                    tex.name = Path.GetFileNameWithoutExtension(file);
                    TextureManager.instance.AddNewTexture(tex);
                }
            }

            if (Directory.Exists(biomes))
            {
                foreach(var file in Directory.GetFiles(biomes).Where(p => p.EndsWith(".json")))
                {
                    JObject biomeData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(file));
                    List<SpawnableFeature> biomeFeatures = new List<SpawnableFeature>();
                    foreach(JObject feature in biomeData["features"])
                    {
                        if (Directory.Exists(features) && File.Exists(features + feature["feature"] + ".json"))
                        {
                            JObject featureData = JsonConvert.DeserializeObject<JObject>(File.ReadAllText(features + feature["feature"] + ".json"));
                            List<FeatureBlock> fBlocks = new List<FeatureBlock>();
                            foreach(JObject block in featureData["blocks"])
                            {
                                FeatureBlock b = new FeatureBlock
                                {
                                    blockId = (short)block["blockId"],
                                    relativePosition = new Vector3((float)block["relativePosition"]["x"], (float)block["relativePosition"]["y"], (float)block["relativePosition"]["z"])
                                };

                                fBlocks.Add(b);
                            }

                            Feature f = ScriptableObject.CreateInstance<Feature>();
                            f.blocks = fBlocks.ToArray();

                            biomeFeatures.Add(new SpawnableFeature
                            {
                                feature = f,
                                commonness = (int)feature["commonness"]
                            });
                        }
                    }

                    Biome biome = ScriptableObject.CreateInstance<Biome>();

                    biome.biomeName = (string)biomeData["name"];
                    biome.commonness = (float)biomeData["commonness"];
                    biome.foiliageColor = new Color
                    {
                        r = (float)biomeData["foliage_color"]["r"],
                        g = (float)biomeData["foliage_color"]["g"],
                        b = (float)biomeData["foliage_color"]["b"],
                        a = (float)biomeData["foliage_color"]["a"],
                    };
                    biome.topBlockId = (short)biomeData["topBlockId"];
                    biome.middleBlockId = (short)biomeData["middleBlockId"];
                    biome.bottomBlockId = (short)biomeData["bottomBlockId"];
                    biome.spawnAbleFeatures = biomeFeatures.ToArray();

                    BlocksManager.Instance.allBiomes.Add(biome);
                    Debug.Log("Registered biome " + biome.biomeName);
                }
            }
        }

        TextureManager.instance.GenerateTextureAtlasFromResources();
    }

    public DatapackData[] GetDatapacks()
    {
        List<DatapackData> dts = new List<DatapackData>();

        foreach (var dir in Directory.GetDirectories(Application.persistentDataPath + dpPath))
        {
            DatapackData data = new DatapackData();
            data.folder_name = Path.GetFileName(dir);
            data.biomeCount = Directory.GetFiles(dir + "\\biomes\\").Where(p => p.EndsWith(".json")).Count();
            data.featureCount = Directory.GetFiles(dir + "\\features\\").Where(p => p.EndsWith(".json")).Count();
            data.enabled = disabledPacks.Contains(data.folder_name);

            dts.Add(data);
        }

        return dts.ToArray();
    }
}

public struct DatapackData
{
    public string folder_name;
    public int biomeCount;
    public int featureCount;
    public bool enabled;
}