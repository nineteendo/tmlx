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
    public Texture2D inputTexture;
    public Texture2D outputTexture;

    public int exitStatus;
}

[Serializable]
public struct BtmlTestSettings
{
    public bool checkOutput;
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
            BtmlTest test = new();
            BtmlTestSettings testSettings = JsonUtility.FromJson<BtmlTestSettings>(Resources.Load<TextAsset>(testPath + "testSettings").text);
            test.inputTexture = Resources.Load<Texture2D>(testPath + "inputTexture");
            if (testSettings.checkOutput)
            {
                test.outputTexture = Resources.Load<Texture2D>(testPath + "outputTexture");
            }

            test.exitStatus = testSettings.exitStatus;
            tests[testIndex] = test;
        }

        return new BtmlLevel() { tests = tests, code = code, solution = solution };
    }
}
