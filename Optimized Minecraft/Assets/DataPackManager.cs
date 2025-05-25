using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class DataPackManager : MonoBehaviour
{
    public static DataPackManager Instance;
    private static string dpPath = "\\datapacks\\";

    private void Awake()
    {
        Instance = this;
    }

    public void ReloadAllPacks()
    {
        foreach (var dir in Directory.GetDirectories(Application.persistentDataPath + dpPath))
        {
            string biomes = dir + "\\biomes\\";
            string features = dir + "\\features\\";

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
    }
}
