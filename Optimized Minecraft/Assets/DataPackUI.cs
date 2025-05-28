using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DataPackUI : MonoBehaviour
{
    public TMP_Text packName;
    public TMP_Text detail;
    public Toggle toggle;
    DatapackData dp;

    public void Init(DatapackData data)
    {
        dp = data;
        packName.text = data.folder_name;
        detail.text = $"Biomes: {data.biomeCount}\tFeatures: {data.featureCount}\tBlocks: {data.blockCount}";
        toggle.isOn = !data.enabled;
        toggle.onValueChanged.AddListener(_ => OnTogglePressed());
    }

    public void OnTogglePressed()
    {
        if (DataPackManager.Instance.disabledPacks.Contains(dp.folder_name))
        {
            DataPackManager.Instance.disabledPacks.Remove(dp.folder_name);
        }
        else
        {
            DataPackManager.Instance.disabledPacks.Add(dp.folder_name);
        }

        MainMenuUI.Instance.RefreshDataPacks();
    }
}
