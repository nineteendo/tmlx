using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(Image), typeof(RectTransform))]
public class BtmlRuntime : MonoBehaviour
{
    public static readonly Color32 COLOR_COVERED_ENTIRELY = new(0x00, 0xff, 0x00, 0x33);
    public static readonly Color32 COLOR_COVERED_HALF = new(0xff, 0xa5, 0x00, 0x33);
    public static readonly Color32 COLOR_COVERED_NEVER = new(0xff, 0x00, 0x00, 0x33);
    public static readonly Color32 COLOR_SOLUTION_NORMAL = new(0x32, 0x32, 0x32, 0xff);
    public static readonly Color32 COLOR_SOLUTION_UNKNOWN = Color.green;
    public static readonly Color32 COLOR_PIXEL_OFF = Color.white;
    public static readonly Color32 COLOR_PIXEL_OFF_SELECTED = new(0xaa, 0xff, 0xff, 0xff);
    public static readonly Color32 COLOR_PIXEL_ON = Color.cyan;
    public static readonly Color32 COLOR_PIXEL_ON_SELECTED = new(0x55, 0xff, 0xff, 0xff);

    public const int CANVAS_HEIGHT = 30;
    public const int CANVAS_WIDTH = 30;
    public const int LEVEL_COUNT = 18;
    public const float MAX_IPF = 3000000f;
    public const float NORMAL_IPS = 10f;
    public const float TURBO_MULTIPLIER = 18000000f;
    public const float UPDATE_INTERVAL = .5f;

    public Button menuButton;
    public Button nextButton;
    public Button replayButton;
    public Button slotAuthorButton;
    public Button slotAutoButton;
    public Button slotBestButton;
    public GameObject levelEndedMenuOverlay;
    public GameObject[] stars;
    public Text congratulationsText;
    public Text errorText;
    public Text infoText;
    public Text levelEndedText;
    public CodeEditor codeEditor;
    public Toggle optimiseToggle;
    public Toggle pauseToggle;
    public Toggle playToggle;
    public Toggle turboToggle;

    private BtmlAction action;
    private BtmlInstruction[] instructions;
    private BtmlLevel level;
    private BtmlTest test;
    private BtmlTest[] tests;
    private Color32[] canvasColors;
    private HashSet<int> breakpoints;
    private List<SaveLevel> saveLevels;
    private SaveLevel saveLevel;
    private Texture2D canvasTexture;

    private bool[] coveredBlackBranches;
    private bool[] coveredWhiteBranches;
    private float elapsedTime;
    private float maxIpf;
    private float normalIps;
    private float queuedInstructions;
    private float targetIps;
    private float totalInstructions;
    private float turboMultiplier;
    private int canvasHeight;
    private int canvasWidth;
    private int canvasTextureOffset;
    private int canvasTextureX;
    private int exitStatus = -1;
    private int instructionCount;
    private int instructionIndex;
    private int ipf;
    private static int levelIndex;
    private int testIndex;

    public void Back()
    {
        SceneManager.LoadScene("Menu");
    }

    public static void LoadLevel(int newLevelIndex)
    {
        levelIndex = newLevelIndex;
        SceneManager.LoadScene("Level");
    }

    public void LoadSlot(int slotIndex)
    {
        codeEditor.inputField.text = slotIndex == 0
            ? level.code
            : slotIndex == 1
                ? saveLevel.autoSave
                : slotIndex == 2
                    ? level.solution
                    : saveLevel.bestSave;
    }

    public void Step()
    {
        queuedInstructions = pauseToggle.isOn ? 1f : 0f;
        pauseToggle.isOn = playToggle.isOn = true;
        targetIps = 0f;
    }

    public void ToggleOptimise()
    {
        PlayerPrefs.SetInt("optimised", optimiseToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void TogglePause()
    {
        ipf = 0;
        queuedInstructions = 0f;
        targetIps = pauseToggle.isOn
            ? 0f
            : normalIps * (turboToggle.isOn ? turboMultiplier : 1f);
    }

    public void TogglePlay()
    {
        if (!playToggle.isOn)
        {
            infoText.text = "";
            codeEditor.MarkedLineIndex = -1;
            codeEditor.MarkedLines = null;
            pauseToggle.isOn = false;
            testIndex = 0;
            LoadTest(true);
            instructions = null;
            return;
        }

        if (!BtmlCompiler.Compile(codeEditor.inputField.text, optimiseToggle.isOn, out instructions, out instructionCount, out string error))
        {
            errorText.text = error;
            return;
        }

        coveredBlackBranches = new bool[instructions.Length];
        coveredWhiteBranches = new bool[instructions.Length];
        SetupInstructionIndex();
        codeEditor.MarkedLineIndex = instructionIndex;
        errorText.text = "";
        slotAutoButton.interactable = true;
        saveLevel.autoSave = codeEditor.inputField.text;
        SaveFunctions.LoadGame().levels[levelIndex] = saveLevel;
        SaveFunctions.SaveGame();
        elapsedTime = totalInstructions = queuedInstructions = 0f;
    }

    public void ToggleTurbo()
    {
        queuedInstructions = 0f;
        if (!pauseToggle.isOn)
        {
            targetIps = normalIps * (turboToggle.isOn ? turboMultiplier : 1f);
        }

        PlayerPrefs.SetInt("turbo", turboToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void SelectPixel()
    {
        if (canvasTextureOffset >= 0 && canvasTextureOffset < canvasColors.Length && canvasTextureX >= 0 && canvasTextureX < canvasWidth)
        {
            int canvasColorIndex = canvasTextureOffset + canvasTextureX;
            Color32 canvasColor = canvasColors[canvasColorIndex];
            canvasColors[canvasColorIndex] = canvasColor.r == COLOR_PIXEL_ON.r || canvasColor.r == COLOR_PIXEL_ON_SELECTED.r ? COLOR_PIXEL_ON_SELECTED : COLOR_PIXEL_OFF_SELECTED;
        }
    }

    private void SetupInstructionIndex()
    {
        int newInstructionIndex = instructions != null && instructions[0].instructionType == BtmlInstructionType.nothing ? instructions[0].whiteAction.gotoLineIndex : 0;
        if (newInstructionIndex >= 0)
        {
            instructionIndex = newInstructionIndex;
            return;
        }

        int executedInstructions = 0;
        Exit(-(newInstructionIndex + 1), ref executedInstructions);
    }

    private void UnSelectPixel()
    {
        if (canvasTextureOffset >= 0 && canvasTextureOffset < canvasColors.Length && canvasTextureX >= 0 && canvasTextureX < canvasWidth)
        {
            int canvasColorIndex = canvasTextureOffset + canvasTextureX;
            Color32 canvasColor = canvasColors[canvasColorIndex];
            canvasColors[canvasColorIndex] = canvasColor.r == COLOR_PIXEL_ON.r || canvasColor.r == COLOR_PIXEL_ON_SELECTED.r ? COLOR_PIXEL_ON : COLOR_PIXEL_OFF;
        }
    }

    private void LoadTest(bool apply)
    {
        test = tests[testIndex];
        Texture2D inputTexture = test.inputTexture;
        canvasColors = Enumerable.Repeat(COLOR_PIXEL_OFF, canvasHeight * canvasWidth).ToArray();
        Color32[] inputColors = inputTexture.GetPixels32();
        int inputTextureHeight = Mathf.Min(inputTexture.height, canvasHeight);
        int inputTextureWidth = Mathf.Min(inputTexture.width, canvasWidth);
        canvasTextureOffset = (canvasHeight - inputTextureHeight) / 2 * canvasWidth;
        canvasTextureX = (canvasWidth - inputTextureWidth) / 2;
        for (int inputTextureY = 0; inputTextureY < inputTextureHeight; inputTextureY++)
        {
            Array.Copy(inputColors, inputTextureY * inputTextureWidth, canvasColors, (inputTextureY * canvasWidth) + canvasTextureOffset + canvasTextureX, inputTextureWidth);
        }

        canvasTextureOffset += (inputTexture.height - 1) * canvasWidth;
        SetupInstructionIndex();
        exitStatus = -1;
        if (breakpoints.Contains(instructionIndex))
        {
            pauseToggle.isOn = true;
        }

        if (apply)
        {
            SelectPixel();
            canvasTexture.SetPixels32(canvasColors);
            canvasTexture.Apply();
        }
    }

    private void Start()
    {
        breakpoints = codeEditor.breakpoints;
        Save save = SaveFunctions.LoadGame();
        saveLevels = save.levels;
        saveLevel = saveLevels[levelIndex];
        level = BtmlLoader.Load(levelIndex);
        codeEditor.inputField.text = saveLevel.autoSave ?? level.code;
#if UNITY_EDITOR
        // Unlock author solution for debugging
        slotAuthorButton.interactable = true;
#else
        slotAuthorButton.interactable = saveLevel.starCount == 3;
#endif
        slotAutoButton.interactable = saveLevel.autoSave != null;
        slotBestButton.interactable = saveLevel.bestSave != null;
        tests = level.tests;
        maxIpf = PlayerPrefs.GetFloat("maxIpf", MAX_IPF);
        normalIps = targetIps = PlayerPrefs.GetFloat("normalIps", NORMAL_IPS);
        turboMultiplier = PlayerPrefs.GetFloat("turboMultiplier", TURBO_MULTIPLIER);
        optimiseToggle.isOn = PlayerPrefs.GetInt("optimised", 1) == 1;
        turboToggle.isOn = PlayerPrefs.GetInt("turbo", 0) == 1;
        Material material = GetComponent<Image>().material;
        ShaderFunctions.SetShader(material, PlayerPrefs.GetInt("paletteShaderIndex", 0));
        ShaderFunctions.SetPalette(material, PlayerPrefs.GetInt("palettePackIndex", 0), PlayerPrefs.GetInt("paletteIndex", 0));
        ShaderFunctions.SetInvert(material, PlayerPrefs.GetInt("invertPalette", 0) == 1);
        canvasHeight = PlayerPrefs.GetInt("canvasHeight", CANVAS_HEIGHT);
        canvasWidth = PlayerPrefs.GetInt("canvasWidth", CANVAS_WIDTH);
        ShaderFunctions.SetDarkFilterLevel(material, PlayerPrefs.GetFloat("darkFilterLevel", 0));
        canvasTexture = new Texture2D(canvasWidth, canvasHeight, TextureFormat.R8, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        LoadTest(true);
        GetComponent<Image>().material.mainTexture = canvasTexture;
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 sizeDelta = rectTransform.sizeDelta;
        float maxDimension = Mathf.Max(sizeDelta.x, sizeDelta.y);
        sizeDelta.x = canvasHeight <= canvasWidth ? maxDimension : maxDimension * canvasWidth / canvasHeight;
        sizeDelta.y = canvasHeight >= canvasWidth ? maxDimension : maxDimension * canvasHeight / canvasWidth;
        rectTransform.sizeDelta = sizeDelta;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel")) // Player pressed ESCAPE or BACK
        {
            Back();
        }

        queuedInstructions += targetIps * Time.unscaledDeltaTime;
        ipf = Mathf.FloorToInt(Mathf.Min(queuedInstructions, maxIpf));
        int executedInstructions = 0;
        if (!playToggle.isOn || instructions == null || HasLevelEnded(ref executedInstructions))
        {
            return;
        }

        UnSelectPixel();
        ref BtmlAction action = ref this.action;
        bool hasBreakpoints = breakpoints.Count > 0;
        int canvasColorsLength = canvasColors.Length;
        for (; executedInstructions < ipf; executedInstructions++)
        {
            int canvasColorIndex = canvasTextureOffset + canvasTextureX;
            if (canvasColors[canvasColorIndex].r == COLOR_PIXEL_ON.r)
            {
                action = ref instructions[instructionIndex].blackAction;
                coveredBlackBranches[instructionIndex] = true;
            }
            else
            {
                action = ref instructions[instructionIndex].whiteAction;
                coveredWhiteBranches[instructionIndex] = true;
            }

            canvasColors[canvasColorIndex] = action.writeColor;
            switch (action.moveDirection)
            {
                case BtmlDirection.nowhere:
                    break;
                case BtmlDirection.up:
                    canvasTextureOffset += canvasWidth;
                    if (canvasTextureOffset >= canvasColorsLength)
                    {
                        goto default;
                    }

                    break;
                case BtmlDirection.down:
                    canvasTextureOffset -= canvasWidth;
                    if (canvasTextureOffset < 0)
                    {
                        goto default;
                    }

                    break;
                case BtmlDirection.left:
                    if (--canvasTextureX < 0)
                    {
                        goto default;
                    }

                    break;
                case BtmlDirection.right:
                    if (++canvasTextureX >= canvasWidth)
                    {
                        goto default;
                    }

                    break;
                default:
                    Exit(2, ref executedInstructions);
                    continue;
            }

            int newInstructionIndex = action.gotoLineIndex;
            if (newInstructionIndex < 0)
            {
                Exit(-(newInstructionIndex + 1), ref executedInstructions);
                continue;
            }

            instructionIndex = newInstructionIndex;
            if (hasBreakpoints && breakpoints.Contains(instructionIndex))
            {
                pauseToggle.isOn = true;
            }
        }

        if (executedInstructions > 0)
        {
            codeEditor.MarkedLineIndex = instructionIndex;
            totalInstructions += executedInstructions;
            queuedInstructions -= executedInstructions;
            SelectPixel();
            canvasTexture.SetPixels32(canvasColors);
            canvasTexture.Apply();
        }

        elapsedTime += Time.unscaledDeltaTime;
        if (exitStatus <= -1 && elapsedTime > UPDATE_INTERVAL && totalInstructions > 0f)
        {
            infoText.text = $"{totalInstructions / elapsedTime:F1} IPS";
            elapsedTime = totalInstructions = 0f;
        }
    }

    private void Exit(int newExitStatus, ref int executedInstructions)
    {
        exitStatus = newExitStatus;
        infoText.text = $"EXIT {exitStatus}";
        executedInstructions++;
        _ = HasLevelEnded(ref executedInstructions);
        executedInstructions--;
    }

    private bool AreArraysEqual(Color32[] array1, int startIndexArray1, Color32[] array2, int startIndexArray2, int length)
    {
        if (startIndexArray1 + length > array1.Length || startIndexArray2 + length > array2.Length)
        {
            return false;
        }

        for (int i = 0; i < length; i++)
        {
            if (array1[startIndexArray1 + i].r != array2[startIndexArray2 + i].r)
            {
                return false;
            }
        }

        return true;
    }

    private int GetStartIndex()
    {
        int canvasColorsLength = canvasColors.Length;
        for (int startIndex = 0; startIndex < canvasWidth; startIndex++)
        {
            for (int offset = 0; offset < canvasColorsLength; offset += canvasWidth)
            {
                if (canvasColors[offset + startIndex].r == COLOR_PIXEL_ON.r)
                {
                    return startIndex;
                }
            }
        }

        return 0;
    }

    private bool HasLevelEnded(ref int executedInstructions)
    {
        if (testIndex >= tests.Length)
        {
            return true;
        }

        if (exitStatus <= -1 || executedInstructions >= ipf)
        {
            return false;
        }

        if (IsOutputCorrect() && exitStatus == test.exitStatus && ++testIndex < tests.Length)
        {
            LoadTest(false);
            executedInstructions++;
            return false;
        }

        levelEndedMenuOverlay.SetActive(true);
        int starCount;
        if (testIndex < tests.Length)
        {
            levelEndedText.text = $"Unit Test {testIndex + 1} Failed!";
            congratulationsText.text = "";
            starCount = 0;
        }
        else
        {
            int totalCoveredBranchesCount = 0;
            Color32[] markedLines = new Color32[instructions.Length];
            for (int instructionIndex = 0; instructionIndex < instructions.Length; instructionIndex++)
            {
                BtmlInstructionType instructionType = instructions[instructionIndex].instructionType;
                if (instructionType == BtmlInstructionType.nothing)
                {
                    continue;
                }

                int coveredBranchesCount = instructionType != BtmlInstructionType.conditional
                    ? coveredBlackBranches[instructionIndex] || coveredWhiteBranches[instructionIndex] ? 2 : 0
                    : (coveredBlackBranches[instructionIndex] ? 1 : 0) + (coveredWhiteBranches[instructionIndex] ? 1 : 0);

                totalCoveredBranchesCount += coveredBranchesCount;
                markedLines[instructionIndex] = coveredBranchesCount == 0
                    ? COLOR_COVERED_NEVER
                    : coveredBranchesCount == 1
                        ? COLOR_COVERED_HALF
                        : COLOR_COVERED_ENTIRELY;
            }

            int coveragePercentage = instructionCount == 0 ? 100 : 100 * totalCoveredBranchesCount / (2 * instructionCount);
            levelEndedText.text = $"{coveragePercentage}% Coverage";
            codeEditor.MarkedLines = markedLines;
            starCount = !BtmlCompiler.Compile(level.solution, false, out _, out int targetInstructionCount, out _) || instructionCount <= targetInstructionCount
                ? 3
                : instructionCount <= targetInstructionCount * 1.5f
                    ? 2
                    : instructionCount <= targetInstructionCount * 2f
                        ? 1
                        : 0;

            congratulationsText.color = instructionCount >= targetInstructionCount ? COLOR_SOLUTION_NORMAL : COLOR_SOLUTION_UNKNOWN;
            congratulationsText.text = instructionCount > targetInstructionCount
                ? ""
                : instructionCount == targetInstructionCount
                    ? "You found the shortest solution!"
                    : "Congratulations, you found an unknown solution.";

            saveLevel.starCount = Mathf.Max(saveLevel.starCount, starCount);
            if (levelIndex + 1 >= saveLevels.Count)
            {
                saveLevels.Add(new SaveLevel());
            }

            if (saveLevel.bestSave == null || !BtmlCompiler.Compile(level.solution, false, out _, out int bestInstructionCount, out _) || instructionCount < bestInstructionCount)
            {
                saveLevel.bestSave = saveLevel.autoSave;
            }

#if !UNITY_EDITOR
            slotAuthorButton.interactable = saveLevel.starCount == 3;
#endif
            slotBestButton.interactable = true;
            SaveFunctions.LoadGame().levels[levelIndex] = saveLevel;
            SaveFunctions.SaveGame();
        }

        for (int starIndex = 0; starIndex < stars.Length; starIndex++)
        {
            stars[starIndex].SetActive(starIndex < starCount);
        }

#if UNITY_EDITOR
        // Add level 0 for debugging
        nextButton.gameObject.SetActive(levelIndex + 1 <= LEVEL_COUNT);
#else
        nextButton.gameObject.SetActive(levelIndex + 1 < LEVEL_COUNT);
#endif
        nextButton.interactable = nextButton.gameObject.activeSelf && levelIndex + 1 < saveLevels.Count;
        nextButton.onClick.RemoveAllListeners();
        if (nextButton.gameObject.activeSelf)
        {
            nextButton.onClick.AddListener(() => LoadLevel(levelIndex + 1));
        }

        EventSystem.current.SetSelectedGameObject(testIndex < tests.Length || starCount < 3
            ? replayButton.gameObject
            : nextButton.gameObject.activeSelf
                ? nextButton.gameObject
                : menuButton.gameObject
        );
        instructions = null;
        ipf = 0;
        queuedInstructions = 0f;
        return true;
    }

    private bool IsOutputCorrect()
    {
        if (test.outputTexture == null)
        {
            return true;
        }

        int startX = GetStartIndex();
        int startY = Array.FindIndex(canvasColors, color => color.r == COLOR_PIXEL_ON.r) / canvasWidth;
        int startOffset = startY * canvasWidth;
        Texture2D outputTexture = test.outputTexture;
        Color32[] outputColors = outputTexture.GetPixels32();
        int outputTextureHeight = outputTexture.height;
        int outputTextureWidth = outputTexture.width;
        if (canvasWidth < startX + outputTextureWidth || canvasHeight < startY + outputTextureHeight)
        {
            return false;
        }

        for (int outputTextureY = 0; outputTextureY < outputTextureHeight; outputTextureY++)
        {
            if (!AreArraysEqual(canvasColors, (outputTextureY * canvasWidth) + startOffset + startX, outputColors, outputTextureY * outputTextureWidth, outputTextureWidth))
            {
                return false;
            }
        }

        return true;
    }
}
