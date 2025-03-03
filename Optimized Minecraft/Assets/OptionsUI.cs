using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class OptionsUI : MonoBehaviour
{
    public TMP_Text frustumCulling;

    private void Start()
    {
        frustumCulling.text = $"Frustum culling [{LevelController.GetBoolText(LevelController.instance.s_frustumCulling)}]";
    }

    private void OnEnable()
    {
        foreach(Transform t in transform)
        {
            t.gameObject.SetActive(false);
        }

        transform.GetChild(0).gameObject.SetActive(true);
    }

    public void ToggleFC()
    {
        LevelController.instance.s_frustumCulling = !LevelController.instance.s_frustumCulling;
        frustumCulling.text = $"Frustum culling [{LevelController.GetBoolText(LevelController.instance.s_frustumCulling)}]";
    }
}
