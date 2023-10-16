using System;
using UnityEngine;

[Serializable]
public struct ShaderMappingObject
{
    public string[] shaderMapping;
}

public static class ShaderFunctions
{
    private static string[] shaderMapping;

    public static void SetDarkFilterLevel(Material material, float darkFilterLevel)
    {
        material.SetFloat("_DarkFilterLevel", darkFilterLevel);
    }

    public static void SetInvert(Material material, bool invert)
    {
        material.SetFloat("_InvertColors", invert ? 1f : 0f);
        if (invert)
        {
            material.EnableKeyword("INVERT_COLORS");
        }
        else
        {
            material.DisableKeyword("INVERT_COLORS");
        }
    }

    public static void SetPalette(Material material, int palettePackIndex, int paletteIndex)
    {
        PalettePack palettePack = PaletteFunctions.LoadPalettePacks()[palettePackIndex];
        material.SetTexture("_ColorMap", Resources.Load<Texture2D>($"Palettes/{palettePack.packId}/{palettePack.paletteMapping[(2 * paletteIndex) + 1]}"));
    }

    public static void SetShader(Material material, int paletteShaderIndex)
    {
        material.shader = Resources.Load<Shader>($"Shaders/{LoadShaders()[(2 * paletteShaderIndex) + 1]}");
    }


    public static string[] LoadShaders()
    {
        shaderMapping ??= JsonUtility.FromJson<ShaderMappingObject>(Resources.Load<TextAsset>("Shaders/shaderMapping").text).shaderMapping;
        return shaderMapping;
    }
}