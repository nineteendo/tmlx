using System;
using UnityEngine;

[Serializable]
public struct ShaderMappingObject
{
    public string[] shaderMapping;
}

public static class ShaderFunctions
{
    static string[] shaderMapping;

    public static void SetDarkFilterLevel(Material material, float darkFilterLevel)
    {
        material.SetFloat("_DarkFilterLevel", darkFilterLevel);
    }

    public static void SetInvert(Material material, bool invert)
    {
        material.SetFloat("_FlipHorizontal", invert ? 0f: 1f);
        if (invert)
            material.DisableKeyword("FLIP_HORIZONTAL");
        else
            material.EnableKeyword("FLIP_HORIZONTAL");
    }

    public static void SetPalette(Material material, int palettePackIndex, int paletteIndex)
    {
        PalettePack palettePack = PaletteFunctions.LoadPalettePacks()[palettePackIndex];
        material.SetTexture("_ColorMap", Resources.Load<Texture2D>($"Palettes/{palettePack.packId}/{palettePack.paletteMapping[2 * paletteIndex + 1]}"));
    }

    public static void SetShader(Material material, int paletteShaderIndex)
    {
        material.shader = Resources.Load<Shader>($"Shaders/{LoadShaders()[2 * paletteShaderIndex + 1]}");
    }


    public static string[] LoadShaders()
    {
        shaderMapping ??= JsonUtility.FromJson<ShaderMappingObject>(Resources.Load<TextAsset>("Shaders/shaderMapping").text).shaderMapping;
        return shaderMapping;
    }
}