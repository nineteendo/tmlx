using System;
using UnityEngine;

[Serializable]
public struct PalettePack
{
    public string packName;
    public string packId;
    public string link;
    public string[] paletteMapping;
}

[Serializable]
public struct PalettePacksObject
{
    public PalettePack[] palettePacks;
}

public static class PaletteFunctions
{
    static PalettePack[] palettePacks;

    public static PalettePack[] LoadPalettePacks()
    {
        palettePacks ??= JsonUtility.FromJson<PalettePacksObject>(Resources.Load<TextAsset>("Palettes/palettePacks").text).palettePacks;
        return palettePacks;
    }
}