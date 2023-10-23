using System;
using UnityEngine;

public struct TmlxLevel
{
    public TmlxTest[] tests;

    public string code;
    public string solution;
}

[Serializable]
public struct TmlxLevelSettings
{
    public int testCount;
}

public struct TmlxTest
{
    public Texture2D inputTexture;
    public Texture2D outputTexture;

    public int exitStatus;
}

[Serializable]
public struct TmlxTestSettings
{
    public bool checkOutput;
    public int exitStatus;
}


public static class TmlxLoader
{
    public static TmlxLevel Load(int levelIndex)
    {
#if UNITY_EDITOR
        // Add level 0 for debugging
        string levelPath = $"Levels/level {levelIndex}/";
#else
        string levelPath = $"Levels/level {levelIndex + 1}/";
#endif
        TmlxLevelSettings levelSettings = JsonUtility.FromJson<TmlxLevelSettings>(Resources.Load<TextAsset>(levelPath + "levelSettings").text);
        string code = Resources.Load<TextAsset>(levelPath + "code").text;
        string solution = Resources.Load<TextAsset>(levelPath + "solution").text;
        TmlxTest[] tests = new TmlxTest[levelSettings.testCount];
        string testsPath = levelPath + "tests/";
        for (int testIndex = 0; testIndex < tests.Length; testIndex++)
        {
            string testPath = testsPath + $"test {testIndex + 1}/";
            TmlxTest test = new();
            TmlxTestSettings testSettings = JsonUtility.FromJson<TmlxTestSettings>(Resources.Load<TextAsset>(testPath + "testSettings").text);
            test.inputTexture = Resources.Load<Texture2D>(testPath + "inputTexture");
            if (testSettings.checkOutput)
            {
                test.outputTexture = Resources.Load<Texture2D>(testPath + "outputTexture");
            }

            test.exitStatus = testSettings.exitStatus;
            tests[testIndex] = test;
        }

        return new TmlxLevel() { tests = tests, code = code, solution = solution };
    }
}
