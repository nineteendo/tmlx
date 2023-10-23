using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TokenColoring
{
    public Color32 tokenColor;

    public string tokenType;
}

[Serializable]
[CreateAssetMenu(fileName = "New Color Theme", menuName = "Tmlx/Color Theme")]
public class ColorTheme : ScriptableObject
{
    public TokenColoring[] tokenColorings = {
        new() {
            tokenColor = new Color32(0x66, 0x66, 0x66, 0xff),
            tokenType = "comment.documentation"
        },
        new() {
            tokenColor = new Color32(0x00, 0x80, 0x00, 0xff),
            tokenType = "comment.normal"
        },
        new() {
            tokenColor = new Color32(0x00, 0x00, 0xff, 0xff),
            tokenType = "identifier.constant"
        },
        new() {
            tokenColor = new Color32(0x79, 0x5E, 0x26, 0xff),
            tokenType = "identifier.function"
        },
        new() {
            tokenColor = new Color32(0xAF, 0x00, 0xDB, 0xff),
            tokenType = "keyword.control"
        },
        new() {
            tokenColor = new Color32(0x09, 0x86, 0x58, 0xff),
            tokenType = "literal.number"
        }
    };

    public Dictionary<string, Color32> GetTokenColoringDictionary()
    {
        Dictionary<string, Color32> tokenColoringDictionary = new();
        foreach (TokenColoring tokenColoring in tokenColorings)
        {
            tokenColoringDictionary[tokenColoring.tokenType] = tokenColoring.tokenColor;
        }

        return tokenColoringDictionary;
    }
}