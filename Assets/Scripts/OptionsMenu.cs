using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public Toggle invertPaletteToggle;
    public InputFieldSlider canvasHeightInputFieldSlider;
    public InputFieldSlider canvasWidthInputFieldSlider;
    public InputFieldSlider darkFilterLevelInputFieldSlider;
    public InputFieldSlider maxIpfInputFieldSlider;
    public InputFieldSlider normalIpsInputFieldSlider;
    public InputFieldSlider turboMultiplierInputFieldSlider;
    public PreviewDropdown palettePackPreviewDropdown;
    public PreviewDropdown palettePreviewDropdown;
    public PreviewDropdown paletteShaderPreviewDropdown;
    public Material screenMaterial;

    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void InvertPalette()
    {
        ShaderFunctions.SetInvert(screenMaterial, invertPaletteToggle.isOn);
        PlayerPrefs.SetInt("invertPalette", invertPaletteToggle.isOn ? 1 : 0);
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

    public void UpdatePack(int paletteIndex)
    {
        ShaderFunctions.SetPalette(screenMaterial, palettePackPreviewDropdown.Value, paletteIndex);
        PlayerPrefs.SetInt("paletteIndex", paletteIndex);
        PlayerPrefs.Save();
    }

    public void UpdatePalettePack(int palettePackIndex)
    {
        palettePreviewDropdown.Value = 0;
        LoadPalettePack();
        PlayerPrefs.SetInt("palettePackIndex", palettePackIndex);
        PlayerPrefs.Save();
    }

    public void UpdateTurboMultiplier()
    {
        PlayerPrefs.SetFloat("turboMultiplier", turboMultiplierInputFieldSlider.slider.value);
        PlayerPrefs.Save();
    }

    public void UpdateShader(int paletteShaderIndex)
    {
        ShaderFunctions.SetShader(screenMaterial, paletteShaderIndex);
        PlayerPrefs.SetInt("paletteShaderIndex", paletteShaderIndex);
        PlayerPrefs.Save();
    }


    private void LoadPalettePack()
    {
        palettePreviewDropdown.Options = PaletteFunctions.LoadPalettePacks()[palettePackPreviewDropdown.Value].paletteMapping.Where((_, index) => index % 2 == 0).ToArray();
        UpdatePack(palettePreviewDropdown.Value);
    }

    private void Start()
    {
        palettePackPreviewDropdown.Options = PaletteFunctions.LoadPalettePacks().Select(pack => pack.packName).ToArray();
        paletteShaderPreviewDropdown.Options = ShaderFunctions.LoadShaders().Where((_, index) => index % 2 == 0).ToArray();
        invertPaletteToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("invertPalette", 0) == 1);
        canvasHeightInputFieldSlider.slider.value = PlayerPrefs.GetInt("canvasHeight", BtmlRuntime.CANVAS_HEIGHT);
        canvasWidthInputFieldSlider.slider.value = PlayerPrefs.GetInt("canvasWidth", BtmlRuntime.CANVAS_WIDTH);
        darkFilterLevelInputFieldSlider.slider.value = PlayerPrefs.GetFloat("darkFilterLevel", 0);
        maxIpfInputFieldSlider.slider.value = PlayerPrefs.GetFloat("maxIpf", BtmlRuntime.MAX_IPF);
        normalIpsInputFieldSlider.slider.value = PlayerPrefs.GetFloat("normalIps", BtmlRuntime.NORMAL_IPS);
        turboMultiplierInputFieldSlider.slider.value = PlayerPrefs.GetFloat("turboMultiplier", BtmlRuntime.TURBO_MULTIPLIER);
        palettePackPreviewDropdown.Value = PlayerPrefs.GetInt("palettePackIndex", 0);
        palettePreviewDropdown.Value = PlayerPrefs.GetInt("paletteIndex", 0);
        paletteShaderPreviewDropdown.Value = PlayerPrefs.GetInt("paletteShaderIndex", 0);
        InvertPalette();
        LoadPalettePack();
        UpdateDarkFilterLevel();
        UpdateShader(paletteShaderPreviewDropdown.Value);
        palettePreviewDropdown.onValueChanged.AddListener((paletteIndex) => ShaderFunctions.SetPalette(screenMaterial, palettePackPreviewDropdown.Value, paletteIndex));
        paletteShaderPreviewDropdown.onValueChanged.AddListener((paletteShaderIndex) => ShaderFunctions.SetShader(screenMaterial, paletteShaderIndex));
        palettePackPreviewDropdown.onEndEdit.AddListener(UpdatePalettePack);
        palettePreviewDropdown.onEndEdit.AddListener(UpdatePack);
        paletteShaderPreviewDropdown.onEndEdit.AddListener(UpdateShader);
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel")) // Player pressed ESCAPE or BACK
        {
            Back();
        }
    }
}
