using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Toggle invertColorsToggle;
    public InputFieldSlider canvasHeightInputFieldSlider;
    public InputFieldSlider canvasWidthInputFieldSlider;
    public InputFieldSlider darkFilterLevelInputFieldSlider;
    public InputFieldSlider maxIpfInputFieldSlider;
    public InputFieldSlider normalIpsInputFieldSlider;
    public InputFieldSlider turboMultiplierInputFieldSlider;
    public PreviewDropdown shaderPreviewDropdown;
    public Material screenMaterial;

    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void InvertColors()
    {
        ShaderFunctions.SetInvertColors(screenMaterial, invertColorsToggle.isOn);
        PlayerPrefs.SetInt("invertColors", invertColorsToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void UpdateCanvasHeight()
    {
        PlayerPrefs.SetInt("canvasHeight", Mathf.FloorToInt(canvasHeightInputFieldSlider.slider.value));
        PlayerPrefs.Save();
    }

    public void UpdateCanvasWidth()
    {
        PlayerPrefs.SetInt("canvasWidth", Mathf.FloorToInt(canvasWidthInputFieldSlider.slider.value));
        PlayerPrefs.Save();
    }

    public void UpdateDarkFilterLevel()
    {
        ShaderFunctions.SetDarkFilterLevel(screenMaterial, darkFilterLevelInputFieldSlider.slider.value);
        PlayerPrefs.SetFloat("darkFilterLevel", darkFilterLevelInputFieldSlider.slider.value);
        PlayerPrefs.Save();
    }

    public void UpdateMaxIpf()
    {
        PlayerPrefs.SetFloat("maxIpf", maxIpfInputFieldSlider.slider.value);
        PlayerPrefs.Save();
    }

    public void UpdateNormalIps()
    {
        PlayerPrefs.SetFloat("normalIps", normalIpsInputFieldSlider.slider.value);
        PlayerPrefs.Save();
    }

    public void UpdateTurboMultiplier()
    {
        PlayerPrefs.SetFloat("turboMultiplier", turboMultiplierInputFieldSlider.slider.value);
        PlayerPrefs.Save();
    }

    public void UpdateShader(int shaderIndex)
    {
        ShaderFunctions.SetShader(screenMaterial, shaderIndex);
        PlayerPrefs.SetInt("shaderIndex", shaderIndex);
        PlayerPrefs.Save();
    }

    private void Start()
    {
        shaderPreviewDropdown.Options = ShaderFunctions.LoadShaders();
        invertColorsToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("invertColors", 0) == 1);
        canvasHeightInputFieldSlider.slider.value = PlayerPrefs.GetInt("canvasHeight", TmlxRuntime.CANVAS_HEIGHT);
        canvasWidthInputFieldSlider.slider.value = PlayerPrefs.GetInt("canvasWidth", TmlxRuntime.CANVAS_WIDTH);
        darkFilterLevelInputFieldSlider.slider.value = PlayerPrefs.GetFloat("darkFilterLevel", 0);
        maxIpfInputFieldSlider.slider.value = PlayerPrefs.GetFloat("maxIpf", TmlxRuntime.MAX_IPF);
        normalIpsInputFieldSlider.slider.value = PlayerPrefs.GetFloat("normalIps", TmlxRuntime.NORMAL_IPS);
        turboMultiplierInputFieldSlider.slider.value = PlayerPrefs.GetFloat("turboMultiplier", TmlxRuntime.TURBO_MULTIPLIER);
        shaderPreviewDropdown.Value = PlayerPrefs.GetInt("shaderIndex", 0);
        InvertColors();
        UpdateDarkFilterLevel();
        UpdateShader(shaderPreviewDropdown.Value);
        shaderPreviewDropdown.onValueChanged.AddListener((shaderIndex) => ShaderFunctions.SetShader(screenMaterial, shaderIndex));
        shaderPreviewDropdown.onEndEdit.AddListener(UpdateShader);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel")) // Player pressed ESCAPE or BACK
        {
            Back();
        }
    }
}
