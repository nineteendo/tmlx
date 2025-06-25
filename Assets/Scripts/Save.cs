using System;
using System.Collections.Generic;
#if !UNITY_EDITOR
using System.IO;
#endif
using System.Linq;
#if !UNITY_EDITOR
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
#endif

[Serializable]
public struct Save
{
    public List<SaveLevel> levels;
}

[Serializable]
public struct SaveLevel
{
    public int starCount;
    public string autoSave;
    public string bestSave;
}

public class SaveFunctions
{
    private static Save save;

    public static void SaveGame()
    {
#if !UNITY_EDITOR
        BinaryFormatter binaryFormatter = new();
        FileStream fileStream = File.Create(Path.Combine(Application.persistentDataPath, "btml.sav"));
        binaryFormatter.Serialize(fileStream, save);
        fileStream.Close();
#endif
    }


    public static Save LoadGame()
    {
        if (save.levels != null)
        {
            return save;
        }

#if UNITY_EDITOR
        // Unlock all levels for debugging
        save.levels = Enumerable.Repeat(new SaveLevel(), BtmlRuntime.LEVEL_COUNT + 2).ToList();
        return save;
#else
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "btml.sav")))
        {
            save.levels = new List<SaveLevel> { new() };
            return save;
        }

        BinaryFormatter binaryFormatter = new();
        FileStream fileStream = File.Open(Path.Combine(Application.persistentDataPath, "btml.sav"), FileMode.Open);
        save = (Save)binaryFormatter.Deserialize(fileStream);
        fileStream.Close();
        return save;
#endif
    }
}