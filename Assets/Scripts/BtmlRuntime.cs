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
    public static readonly Color32 COLOR_CONGRATULATIONS = Color.green;
    public static readonly Color32 COLOR_PIXEL_OFF = Color.white;
    public static readonly Color32 COLOR_PIXEL_OFF_SELECTED = new Color32(0xaa, 0xff, 0xff, 0xff);
    public static readonly Color32 COLOR_PIXEL_ON = Color.cyan;
    public static readonly Color32 COLOR_PIXEL_ON_SELECTED = new Color32(0x55, 0xff, 0xff, 0xff);

    public const int LEVEL_COUNT = 6;
    public const float MAX_IPF = 30000000f;
    public const float NORMAL_IPS = 5f;
    public const float TURBO_MULTIPLIER = 18000000f;
    public const float UPDATE_INTERVAL = .5f;

    public Button nextButton;
    public Button[] slotButtons;
    public GameObject levelEndedMenu;
    public GameObject[] stars;
    public Text congratulationsText;
    public Text errorText;
    public Text infoText;
    public Text levelEndedText;
    public CodeEditor codeEditor;
    public Toggle pauseToggle;
    public Toggle playToggle;
    public Toggle turboToggle;

    BtmlInstruction[] program;
    BtmlLevel level;
    BtmlTest test;
    BtmlTest[] tests;
    Color32[] canvasColors;
    SaveLevel saveLevel;
    List<SaveLevel> saveLevels;
    Texture2D canvasTexture;

    float elapsedTime;
    float executedInstructions;
    float ipf;
    float maxIpf;
    float normalIps;
    float queuedInstructions;
    float targetIps;
    float turboMultiplier;
    int canvasTextureHeight;
    int canvasTextureWidth;
    int canvasTextureX;
    int canvasTextureY;
    static int levelIndex;
    int programIndex;
    int returnCode = -1;
    int testIndex;

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
        if (slotIndex == 0)
            codeEditor.inputField.text = level.code;
        else if (slotIndex == 1)
            codeEditor.inputField.text = saveLevel.autoSave;
        else if (slotIndex == 2)
            codeEditor.inputField.text = level.solution;
        else
            codeEditor.inputField.text = saveLevel.shortSave;
    }

    public void Step()
    {
        pauseToggle.isOn = playToggle.isOn = true;
        queuedInstructions = 1f;
        targetIps = 0f;
    }

    public void TogglePause()
    {
        queuedInstructions = 0f;
        if (pauseToggle.isOn)
            targetIps = 0f;
        else
            targetIps = normalIps * (turboToggle.isOn ? turboMultiplier : 1f);
    }

    public void TogglePlay()
    {
        slotButtons[1].interactable = true;
        saveLevel.autoSave = codeEditor.inputField.text;
        SaveFunctions.LoadGame().levels[levelIndex] = saveLevel;
        SaveFunctions.SaveGame();
        testIndex = 0;
        LoadTest();
        elapsedTime = executedInstructions = queuedInstructions = 0f;
        SelectPixel();
        canvasTexture.SetPixels32(canvasColors);
        canvasTexture.Apply();
        if (!playToggle.isOn)
        {
            program = null;
            returnCode = 0;
            return;
        }

        if (BtmlCompiler.Compile(level.extraReturnCodeCount, codeEditor.inputField.text, out program, out string error))
            errorText.text = "";
        else
            errorText.text = error;
    }

    public void ToggleTurbo()
    {
        queuedInstructions = 0f;
        if (!pauseToggle.isOn)
            targetIps = normalIps * (turboToggle.isOn ? turboMultiplier : 1f);

        PlayerPrefs.SetInt("turbo", turboToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }


    void SelectPixel()
    {
        float canvasColorR = canvasColors[canvasTextureY * canvasTexture.width + canvasTextureX].r;
        canvasColors[canvasTextureY * canvasTexture.width + canvasTextureX] = canvasColorR == COLOR_PIXEL_ON.r || canvasColorR == COLOR_PIXEL_ON_SELECTED.r ? COLOR_PIXEL_ON_SELECTED : COLOR_PIXEL_OFF_SELECTED;
    }

    void UnSelectPixel()
    {
        float canvasColorR = canvasColors[canvasTextureY * canvasTexture.width + canvasTextureX].r;
        canvasColors[canvasTextureY * canvasTexture.width + canvasTextureX] = canvasColorR == COLOR_PIXEL_ON.r || canvasColorR == COLOR_PIXEL_ON_SELECTED.r ? COLOR_PIXEL_ON : COLOR_PIXEL_OFF;
    }

    void LoadTest()
    {
        infoText.text = "";
        test = tests[testIndex];
        Texture2D newCanvasTexture = test.canvasTexture;
        canvasColors = newCanvasTexture.GetPixels32();
        canvasTextureHeight = newCanvasTexture.height;
        canvasTextureWidth = newCanvasTexture.width;
        canvasTexture.Reinitialize(canvasTextureWidth, canvasTextureHeight);
        GetComponent<Image>().material.SetTexture("_ColorIdxs", canvasTexture);
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 sizeDelta = rectTransform.sizeDelta;
        float maxDimension = Mathf.Max(sizeDelta.x, sizeDelta.y);
        sizeDelta.x = canvasTextureHeight <= canvasTextureWidth ? maxDimension : maxDimension * canvasTextureWidth / canvasTextureHeight;
        sizeDelta.y = canvasTextureHeight >= canvasTextureWidth ? maxDimension : maxDimension * canvasTextureHeight / canvasTextureWidth;
        rectTransform.sizeDelta = sizeDelta;
        canvasTextureX = canvasTextureY = programIndex = 0;
        returnCode = -1;
    }

    void Start()
    {
        canvasTexture = new Texture2D(2, 2, TextureFormat.R8, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };
        Save save = SaveFunctions.LoadGame();
        saveLevels = save.levels;
        saveLevel = saveLevels[levelIndex];
        level = BtmlLoader.Load(levelIndex);
        slotButtons[1].interactable = saveLevel.autoSave != null;
        slotButtons[2].interactable = saveLevel.starCount == 3;
        slotButtons[3].interactable = saveLevel.shortSave != null;
        codeEditor.inputField.text = saveLevel.autoSave ?? level.code;
        tests = level.tests;
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

    void Update()
    {
        if (Input.GetButtonDown("Cancel")) // Player pressed ESCAPE or BACK
            Back();

        queuedInstructions += targetIps * Time.unscaledDeltaTime;
        ipf = Math.Min(queuedInstructions, MAX_IPF);
        int instruction = 0;
        if (!playToggle.isOn || program == null || LevelEnded(ref instruction))
            return;

        UnSelectPixel();
        for (; instruction < ipf; instruction++)
        {
            int colorIndex = canvasTextureY * canvasTextureWidth + canvasTextureX;
            float canvasColorR = canvasColors[colorIndex].r;
            ref BtmlAction action = ref (canvasColorR == COLOR_PIXEL_ON.r ? ref program[programIndex].blackAction : ref program[programIndex].whiteAction);
            canvasColors[colorIndex] = action.writeColor;
            int newProgramIndex = action.gotoLine;
            if (newProgramIndex < 0)
            {
                returnCode = -newProgramIndex - 1;
                infoText.text = $"RETURN CODE {returnCode}: LINE {programIndex + 1} X {canvasTextureX} Y {canvasTextureY} {(canvasColorR == COLOR_PIXEL_ON.r ? "BLACK" : "WHITE")}";
                instruction++;
                if (LevelEnded(ref instruction))
                    break;

                instruction--;
                continue;
            }

            MoveDirection moveDirection = action.moveDirection;
            if (moveDirection == MoveDirection.Up || moveDirection == MoveDirection.Down)
            {
                canvasTextureY += moveDirection == MoveDirection.Down ? canvasTextureHeight - 1 : 1;
                canvasTextureY %= canvasTextureHeight;
            }
            else
            {
                canvasTextureX += moveDirection == MoveDirection.Left ? canvasTextureWidth - 1 : 1;
                canvasTextureX %= canvasTextureWidth;
            }

            programIndex = newProgramIndex;
        }

        if (ipf > 0)
        {
            SelectPixel();
            canvasTexture.SetPixels32(canvasColors);
            canvasTexture.Apply();
        }

        elapsedTime += Time.unscaledDeltaTime;
        executedInstructions += ipf;
        queuedInstructions -= ipf;
        if (returnCode <= -1 && elapsedTime > UPDATE_INTERVAL && executedInstructions > 0f)
        {
            infoText.text = $"{executedInstructions / elapsedTime:F1} IPS";
            elapsedTime = executedInstructions = 0f;
        }
    }


    bool LevelEnded(ref int instruction)
    {
        if (testIndex >= tests.Length)
            return true;

        if (returnCode <= -1 || instruction >= ipf)
            return false;

        if (returnCode == test.returnCode && ++testIndex < tests.Length)
        {
            LoadTest();
            instruction++;
            return false;
        }

        int instructionCount = program.Length;
        program = null;
        levelEndedMenu.transform.parent.gameObject.SetActive(true);
        levelEndedMenu.SetActive(true);
        EventSystem.current.SetSelectedGameObject(nextButton.gameObject);
        int starCount;
        if (testIndex < tests.Length)
        {
            levelEndedText.text = $"Test {testIndex + 1} Failed!";
            congratulationsText.text = "";
            starCount = 0;
        }
        else
        {
            levelEndedText.text = "Level Completed!";
            int targetInstructionCount = level.solution.Count(c => c == '\n') + 1;
            if (instructionCount > targetInstructionCount * 2f)
                starCount = 0;
            else if (instructionCount > targetInstructionCount * 1.5f)
                starCount = 1;
            else if (instructionCount > targetInstructionCount)
                starCount = 2;
            else
                 starCount = 3;

            if (instructionCount > targetInstructionCount)
                congratulationsText.text = "";
            else if (instructionCount == targetInstructionCount)
                congratulationsText.text = "You found the shortest solution!";
            else if (instructionCount < targetInstructionCount)
            {
                congratulationsText.text = "Congratulations, you found an unknown solution.";
                congratulationsText.color = COLOR_CONGRATULATIONS;
            }

            saveLevel.starCount = Mathf.Max(saveLevel.starCount, starCount);
            if (levelIndex + 1 >= saveLevels.Count)
                saveLevels.Add(new SaveLevel());

            if (saveLevel.shortSave == null || saveLevel.autoSave.Count(c => c == '\n') < saveLevel.shortSave.Count(c => c == '\n'))
                saveLevel.shortSave = saveLevel.autoSave;

            slotButtons[2].interactable = saveLevel.starCount == 3;
            slotButtons[3].interactable = true;
            SaveFunctions.LoadGame().levels[levelIndex] = saveLevel;
            SaveFunctions.SaveGame();
        }
        for (int starIndex = 0; starIndex < stars.Length; starIndex++)
            stars[starIndex].SetActive(starIndex < starCount);

        nextButton.gameObject.SetActive(levelIndex + 1 < saveLevels.Count && levelIndex + 1 < LEVEL_COUNT);
        nextButton.onClick.RemoveAllListeners();
        if (nextButton.gameObject.activeSelf)
            nextButton.onClick.AddListener(() => LoadLevel(levelIndex + 1));

        return true;
    }
}
