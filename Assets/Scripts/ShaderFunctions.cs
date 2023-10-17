using System;
using UnityEngine;

[Serializable]
public struct ShadersObject
{
    public string[] shaders;
}

public static class ShaderFunctions
{
    private static string[] shaders;

    public static void SetDarkFilterLevel(Material material, float darkFilterLevel)
    {
        material.SetFloat("_DarkFilterLevel", darkFilterLevel);
    }

    public static void SetInvertColors(Material material, bool invert)
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

    public static void SetShader(Material material, int shaderIndex)
    {
        material.shader = Resources.Load<Shader>($"Shaders/{LoadShaders()[shaderIndex]}");
    }


    public static string[] LoadShaders()
    {
        shaders ??= JsonUtility.FromJson<ShadersObject>(Resources.Load<TextAsset>("Shaders/shaders").text).shaders;
        return shaders;
    }
}