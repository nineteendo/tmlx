using System.IO;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "tmlx")]
public class TmlxImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        TextAsset subAsset = new(File.ReadAllText(ctx.assetPath));
        ctx.AddObjectToAsset("text", subAsset);
        ctx.SetMainObject(subAsset);
    }
}