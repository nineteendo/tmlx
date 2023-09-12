using System;
using UnityEngine;

public struct BtmlLevel
{
    public BtmlTest[] tests;

    public int extraReturnCodeCount;
    public string code;
    public string solution;
}

[Serializable]
public struct BtmlLevelSettings
{
    public int extraReturnCodeCount;
    public int testCount;
}

public struct BtmlTest
{
    public Texture2D canvasTexture;

    public int returnCode;
}

[Serializable]
public struct BtmlTestSettings
{
    public int returnCode;
}


public static class BtmlLoader
{
    public static BtmlLevel Load(int levelIndex)
    {
        string levelPath = $"Levels/level {levelIndex + 1}/";
        BtmlLevelSettings levelSettings = JsonUtility.FromJson<BtmlLevelSettings>(Resources.Load<TextAsset>(levelPath + "levelSettings").text);
        int extraReturnCodeCount = levelSettings.extraReturnCodeCount;
        string code = Resources.Load<TextAsset>(levelPath + "code").text;
        string solution = Resources.Load<TextAsset>(levelPath + "solution").text;
        BtmlTest[] tests = new BtmlTest[levelSettings.testCount];
        string testsPath = levelPath + "tests/";
        for (int testIndex = 0; testIndex < tests.Length; testIndex++)
        {
            string testPath = testsPath + $"test {testIndex + 1}/";
            BtmlTestSettings testSettings = JsonUtility.FromJson<BtmlTestSettings>(Resources.Load<TextAsset>(testPath + "testSettings").text);
            int returnCode = testSettings.returnCode;
            Texture2D canvasTexture = Resources.Load<Texture2D>(testPath + "canvasTexture");
            tests[testIndex] = new BtmlTest() { canvasTexture = canvasTexture, returnCode = returnCode };
        }

        return new BtmlLevel() { tests = tests, extraReturnCodeCount = extraReturnCodeCount, code = code, solution = solution };
    }
}
