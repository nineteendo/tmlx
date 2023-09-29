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

    public const int LEVEL_COUNT = 14;
    public const float MAX_IPF = 3000000f;
    public const float NORMAL_IPS = 5f;
    public const float TURBO_MULTIPLIER = 36000000f;
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
    private int canvasTextureOffset;
    private int canvasTextureOffsetMax;
    private int canvasTextureWidth;
    private int canvasTextureX;
    private int exitStatus = -1;
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
        codeEditor.inputField.text = slotIndex == 0 ? level.code : slotIndex == 1 ? saveLevel.autoSave : slotIndex == 2 ? level.solution : saveLevel.bestSave;
    }

    public void Step()
    {
        queuedInstructions = pauseToggle.isOn ? 1f : 0f;
        pauseToggle.isOn = playToggle.isOn = true;
        targetIps = 0f;
    }

    public void TogglePause()
    {
        ipf = 0;
        queuedInstructions = 0f;
        targetIps = pauseToggle.isOn ? 0f : normalIps * (turboToggle.isOn ? turboMultiplier : 1f);
    }

    public void TogglePlay()
    {
        codeEditor.MarkedLines = null;
        testIndex = 0;
        LoadTest();
        elapsedTime = totalInstructions = queuedInstructions = 0f;
        SelectPixel();
        canvasTexture.SetPixels32(canvasColors);
        canvasTexture.Apply();
        if (!playToggle.isOn)
        {
            codeEditor.MarkedLineIndex = -1;
            pauseToggle.isOn = false;
            instructions = null;
            return;
        }

        if (!BtmlCompiler.Compile(codeEditor.inputField.text, out instructions, out string error))
        {
            errorText.text = error;
        }
        else
        {
            codeEditor.MarkedLineIndex = 0;
            errorText.text = "";
            slotAutoButton.interactable = true;
            saveLevel.autoSave = codeEditor.inputField.text;
            SaveFunctions.LoadGame().levels[levelIndex] = saveLevel;
            SaveFunctions.SaveGame();
            coveredBlackBranches = new bool[instructions.Length];
            coveredWhiteBranches = new bool[instructions.Length];
        }
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
        int canvasColorIndex = canvasTextureOffset + canvasTextureX;
        if (canvasColorIndex < canvasColors.Length)
        {
            Color32 canvasColor = canvasColors[canvasColorIndex];
            canvasColors[canvasColorIndex] = canvasColor.r == COLOR_PIXEL_ON.r || canvasColor.r == COLOR_PIXEL_ON_SELECTED.r ? COLOR_PIXEL_ON_SELECTED : COLOR_PIXEL_OFF_SELECTED;
        }
    }

    private void UnSelectPixel()
    {
        int canvasColorIndex = canvasTextureOffset + canvasTextureX;
        if (canvasColorIndex < canvasColors.Length)
        {
            Color32 canvasColor = canvasColors[canvasColorIndex];
            canvasColors[canvasColorIndex] = canvasColor.r == COLOR_PIXEL_ON.r || canvasColor.r == COLOR_PIXEL_ON_SELECTED.r ? COLOR_PIXEL_ON : COLOR_PIXEL_OFF;
        }
    }

    private void LoadTest()
    {
        infoText.text = "";
        test = tests[testIndex];
        Texture2D newCanvasTexture = test.canvasTexture;
        canvasColors = newCanvasTexture.GetPixels32();
        int canvasTextureHeight = newCanvasTexture.height;
        canvasTextureWidth = newCanvasTexture.width;
        canvasTextureOffsetMax = canvasTextureHeight * canvasTextureWidth;
        _ = canvasTexture.Reinitialize(canvasTextureWidth, canvasTextureHeight);
        GetComponent<Image>().material.mainTexture = canvasTexture;
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 sizeDelta = rectTransform.sizeDelta;
        float maxDimension = Mathf.Max(sizeDelta.x, sizeDelta.y);
        sizeDelta.x = canvasTextureHeight <= canvasTextureWidth ? maxDimension : maxDimension * canvasTextureWidth / canvasTextureHeight;
        sizeDelta.y = canvasTextureHeight >= canvasTextureWidth ? maxDimension : maxDimension * canvasTextureHeight / canvasTextureWidth;
        rectTransform.sizeDelta = sizeDelta;
        canvasTextureX = canvasTextureOffset = instructionIndex = 0;
        exitStatus = -1;
        if (breakpoints.Contains(instructionIndex))
        {
            pauseToggle.isOn = true;
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
        canvasTexture = new Texture2D(2, 2, TextureFormat.R8, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        LoadTest();
        SelectPixel();
        canvasTexture.SetPixels32(canvasColors);
        canvasTexture.Apply();
        maxIpf = PlayerPrefs.GetFloat("maxIpf", MAX_IPF);
        normalIps = targetIps = PlayerPrefs.GetFloat("normalIps", NORMAL_IPS);
        turboMultiplier = PlayerPrefs.GetFloat("turboMultiplier", TURBO_MULTIPLIER);
        turboToggle.isOn = PlayerPrefs.GetInt("turbo", 0) == 1;
        Material material = GetComponent<Image>().material;
        ShaderFunctions.SetShader(material, PlayerPrefs.GetInt("paletteShaderIndex", 0));
        ShaderFunctions.SetPalette(material, PlayerPrefs.GetInt("palettePackIndex", 0), PlayerPrefs.GetInt("paletteIndex", 0));
        ShaderFunctions.SetInvert(material, PlayerPrefs.GetInt("invertPalette", 0) == 1);
        ShaderFunctions.SetDarkFilterLevel(material, PlayerPrefs.GetFloat("darkFilterLevel", 0));
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
        if (!playToggle.isOn || instructions == null || LevelEnded(ref executedInstructions))
        {
            return;
        }

        UnSelectPixel();
        ref BtmlAction action = ref this.action;
        bool hasBreakpoints = breakpoints.Count > 0;
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
                    canvasTextureOffset += canvasTextureWidth;
                    if (canvasTextureOffset == canvasTextureOffsetMax)
                    {
                        goto default;
                    }

                    break;
                case BtmlDirection.down:
                    canvasTextureOffset -= canvasTextureWidth;
                    if (canvasTextureOffset == -canvasTextureWidth)
                    {
                        goto default;
                    }

                    break;
                case BtmlDirection.left:
                    if (--canvasTextureX == -1)
                    {
                        goto default;
                    }

                    break;
                case BtmlDirection.right:
                    if (++canvasTextureX == canvasTextureWidth)
                    {
                        goto default;
                    }

                    break;
                default:
                    Exit(2, ref executedInstructions);
                    continue;
            }

            int newProgramIndex = action.gotoLineIndex;
            if (newProgramIndex < 0)
            {
                Exit(-(newProgramIndex + 1), ref executedInstructions);
                continue;
            }

            instructionIndex = newProgramIndex;
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
        _ = LevelEnded(ref executedInstructions);
        executedInstructions--;
    }

    private bool LevelEnded(ref int executedInstructions)
    {
        if (testIndex >= tests.Length)
        {
            return true;
        }

        if (exitStatus <= -1 || executedInstructions >= ipf)
        {
            return false;
        }

        if (exitStatus == test.exitStatus && ++testIndex < tests.Length)
        {
            LoadTest();
            executedInstructions++;
            return false;
        }

        int instructionCount = instructions.Length;
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
                int coveredBranchesCount = !instructions[instructionIndex].conditional ? coveredBlackBranches[instructionIndex] || coveredWhiteBranches[instructionIndex] ? 2 : 0 : (coveredBlackBranches[instructionIndex] ? 1 : 0) + (coveredWhiteBranches[instructionIndex] ? 1 : 0);
                totalCoveredBranchesCount += coveredBranchesCount;
                markedLines[instructionIndex] = coveredBranchesCount == 0 ? COLOR_COVERED_NEVER : coveredBranchesCount == 1 ? COLOR_COVERED_HALF : COLOR_COVERED_ENTIRELY;
            }

            int coveragePercentage = 100 * totalCoveredBranchesCount / (2 * instructions.Length);
            levelEndedText.text = $"{coveragePercentage}% Coverage";
            codeEditor.MarkedLines = markedLines;
            int targetInstructionCount = level.solution.Count(c => c == '\n') + 1;
            starCount = instructionCount > targetInstructionCount * 2f
                ? 0
                : instructionCount > targetInstructionCount * 1.5f ? 1 : instructionCount > targetInstructionCount ? 2 : 3;

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

            if (saveLevel.bestSave == null || saveLevel.autoSave.Count(c => c == '\n') < saveLevel.bestSave.Count(c => c == '\n'))
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
        nextButton.gameObject.SetActive(levelIndex + 1 < saveLevels.Count && levelIndex + 1 <= LEVEL_COUNT);
#else
        nextButton.gameObject.SetActive(levelIndex + 1 < saveLevels.Count && levelIndex + 1 < LEVEL_COUNT);
#endif
        nextButton.onClick.RemoveAllListeners();
        if (nextButton.gameObject.activeSelf)
        {
            nextButton.onClick.AddListener(() => LoadLevel(levelIndex + 1));
        }

        EventSystem.current.SetSelectedGameObject(testIndex < tests.Length || starCount < 3
            ? replayButton.gameObject
            : nextButton.gameObject.activeSelf ? nextButton.gameObject : menuButton.gameObject
        );
        instructions = null;
        ipf = 0;
        queuedInstructions = 0f;
        return true;
    }
}
