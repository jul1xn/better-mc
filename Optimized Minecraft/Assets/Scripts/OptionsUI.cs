using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class OptionsUI : MonoBehaviour
{
    public TMP_Text quality;
    public TMP_Text tfculling;
    public TMP_Text antia;
    public TMP_Text frustumCulling;
    public TMP_Text fieldOfView;
    public Slider fieldOfViewSlider;
    public TMP_Text rendText;
    public Slider rendSlider;
    public string[] qualities;
    public string[] antians;

    private void Start()
    {
        frustumCulling.text = $"Frustum culling: {Helper.GetBoolText(LevelController.instance.s_frustumCulling)}";
        fieldOfView.text = $"Fov: {Mathf.Round(LevelController.instance.s_fov)}";
        fieldOfViewSlider.value = LevelController.instance.s_fov;
        quality.text = $"Quality: {qualities[LevelController.instance.s_quality]}";
        QualitySettings.SetQualityLevel(LevelController.instance.s_quality);

        tfculling.text = $"TF culling: {Helper.GetBoolText(LevelController.instance.s_transparentculling)}";
        antia.text = $"Anti Aliasing: {antians[LevelController.instance.s_antia]}";

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

    public void SetQuality()
    {
        LevelController.instance.s_quality++;
        if (LevelController.instance.s_quality >= qualities.Length)
        {
            LevelController.instance.s_quality = 0;
        }

        quality.text = $"Quality: {qualities[LevelController.instance.s_quality]}";
        QualitySettings.SetQualityLevel(LevelController.instance.s_quality);
    }

    public void SetTF()
    {
        LevelController.instance.s_transparentculling = !LevelController.instance.s_transparentculling;
        tfculling.text = $"TF culling: {Helper.GetBoolText(LevelController.instance.s_transparentculling)}";
    }

    public void SetAntiA()
    {
        LevelController.instance.s_antia++;
        if (LevelController.instance.s_antia >= antians.Length)
        {
            LevelController.instance.s_antia = 0;
        }
        antia.text = $"Anti Aliasing: {antians[LevelController.instance.s_antia]}";
    }
}
