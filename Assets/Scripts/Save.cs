using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

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
    public string shortSave;
}

public class SaveFunctions
{
    static Save save;

    public static Save LoadGame()
    {
        if (save.levels != null)
            return save;

        
        if (!File.Exists(Path.Combine(Application.persistentDataPath, "btml.sav")))
        {
            save.levels = new List<SaveLevel> { new SaveLevel() };
            return save;
        }

        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Open(Path.Combine(Application.persistentDataPath, "btml.sav"), FileMode.Open);
        save = (Save)binaryFormatter.Deserialize(fileStream);
        fileStream.Close();
        return save;
    }

    public static void SaveGame()
    {
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Create(Path.Combine(Application.persistentDataPath, "btml.sav"));
        binaryFormatter.Serialize(fileStream, save);
        fileStream.Close();
    }
}