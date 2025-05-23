using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class OptionsUI : MonoBehaviour
{
    public TMP_Text frustumCulling;
    public TMP_Text fieldOfView;
    public Slider fieldOfViewSlider;
    public TMP_Text rendText;
    public Slider rendSlider;

    private void Start()
    {
        frustumCulling.text = $"Frustum culling: {Helper.GetBoolText(LevelController.instance.s_frustumCulling)}";
        fieldOfView.text = $"Fov: {Mathf.Round(LevelController.instance.s_fov)}";
        fieldOfViewSlider.value = LevelController.instance.s_fov;

        rendText.text = $"Render distance: {LevelController.instance.s_rendDist}";
        rendSlider.value = LevelController.instance.s_rendDist;
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
        frustumCulling.text = $"Frustum culling: {Helper.GetBoolText(LevelController.instance.s_frustumCulling)}";
    }

    public void SetFov(float fov)
    {
        LevelController.instance.s_fov = fov;
        fieldOfView.text = $"Fov: {Mathf.Round(LevelController.instance.s_fov)}";
    }

    public void SetRenderDistance(float distance)
    {
        LevelController.instance.s_rendDist = Mathf.RoundToInt(distance);
        rendText.text = $"Render distance: {LevelController.instance.s_rendDist}";
    }
}
