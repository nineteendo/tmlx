using System;
using UnityEngine;

public struct BtmlLevel
{
    public BtmlTest[] tests;

    public string code;
    public string solution;
}

[Serializable]
public struct BtmlLevelSettings
{
    public int testCount;
}

public struct BtmlTest
{
    public Texture2D canvasTexture;

    public int exitStatus;
}

[Serializable]
public struct BtmlTestSettings
{
    public int exitStatus;
}


public static class BtmlLoader
{
    public static BtmlLevel Load(int levelIndex)
    {
#if UNITY_EDITOR
        // Add level 0 for debugging
        string levelPath = $"Levels/level {levelIndex}/";
#else
        string levelPath = $"Levels/level {levelIndex + 1}/";
#endif
        BtmlLevelSettings levelSettings = JsonUtility.FromJson<BtmlLevelSettings>(Resources.Load<TextAsset>(levelPath + "levelSettings").text);
        string code = Resources.Load<TextAsset>(levelPath + "code").text;
        string solution = Resources.Load<TextAsset>(levelPath + "solution").text;
        BtmlTest[] tests = new BtmlTest[levelSettings.testCount];
        string testsPath = levelPath + "tests/";
        for (int testIndex = 0; testIndex < tests.Length; testIndex++)
        {
            string testPath = testsPath + $"test {testIndex + 1}/";
            BtmlTestSettings testSettings = JsonUtility.FromJson<BtmlTestSettings>(Resources.Load<TextAsset>(testPath + "testSettings").text);
            int exitStatus = testSettings.exitStatus;
            Texture2D canvasTexture = Resources.Load<Texture2D>(testPath + "canvasTexture");
            tests[testIndex] = new BtmlTest() { canvasTexture = canvasTexture, exitStatus = exitStatus };
        }

        return new BtmlLevel() { tests = tests, code = code, solution = solution };
    }
}
