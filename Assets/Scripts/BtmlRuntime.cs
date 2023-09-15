using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


[RequireComponent(typeof(Image), typeof(RectTransform))]
public class BtmlRuntime : MonoBehaviour
{
    public static readonly Color32 COLOR_SOLUTION_NORMAL = new(0x32, 0x32, 0x32, 0xff);
    public static readonly Color32 COLOR_SOLUTION_UNKNOWN = Color.green;
    public static readonly Color32 COLOR_PIXEL_OFF = Color.white;
    public static readonly Color32 COLOR_PIXEL_OFF_SELECTED = new(0xaa, 0xff, 0xff, 0xff);
    public static readonly Color32 COLOR_PIXEL_ON = Color.cyan;
    public static readonly Color32 COLOR_PIXEL_ON_SELECTED = new(0x55, 0xff, 0xff, 0xff);

    public const int LEVEL_COUNT = 10;
    public const float MAX_IPF = 40000000f;
    public const float NORMAL_IPS = 5f;
    public const float TURBO_MULTIPLIER = 24000000f;
    public const float UPDATE_INTERVAL = .5f;

    public Button menuButton;
    public Button nextButton;
    public Button[] slotButtons;
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

    private BtmlInstruction[] program;
    private BtmlLevel level;
    private BtmlTest test;
    private BtmlTest[] tests;
    private Color32[] canvasColors;
    private HashSet<int> breakpoints;
    private List<SaveLevel> saveLevels;
    private SaveLevel saveLevel;
    private Texture2D canvasTexture;

    private float elapsedTime;
    private float executedInstructions;
    private float ipf;
    private float maxIpf;
    private float normalIps;
    private float queuedInstructions;
    private float targetIps;
    private float turboMultiplier;
    private int canvasColorIndex;
    private int canvasTextureHeight;
    private int canvasTextureWidth;
    private int canvasTextureX;
    private int canvasTextureY;
    private static int levelIndex;
    private int programIndex;
    private int returnCode = -1;
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
        ipf = queuedInstructions = 0f;
        targetIps = pauseToggle.isOn ? 0f : normalIps * (turboToggle.isOn ? turboMultiplier : 1f);
    }

    public void TogglePlay()
    {
        testIndex = 0;
        LoadTest();
        elapsedTime = executedInstructions = queuedInstructions = 0f;
        SelectPixel();
        canvasTexture.SetPixels32(canvasColors);
        canvasTexture.Apply();
        if (!playToggle.isOn)
        {
            codeEditor.MarkedLineIndex = -1;
            program = null;
            return;
        }

        if (!BtmlCompiler.Compile(level.extraReturnCodeCount, codeEditor.inputField.text, out program, out string error))
        {
            errorText.text = error;
        }
        else
        {
            codeEditor.MarkedLineIndex = 0;
            errorText.text = "";
            slotButtons[1].interactable = true;
            saveLevel.autoSave = codeEditor.inputField.text;
            SaveFunctions.LoadGame().levels[levelIndex] = saveLevel;
            SaveFunctions.SaveGame();
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
        float canvasColorR = canvasColors[canvasColorIndex].r;
        canvasColors[canvasColorIndex] = canvasColorR == COLOR_PIXEL_ON.r || canvasColorR == COLOR_PIXEL_ON_SELECTED.r ? COLOR_PIXEL_ON_SELECTED : COLOR_PIXEL_OFF_SELECTED;
    }

    private void UnSelectPixel()
    {
        float canvasColorR = canvasColors[canvasColorIndex].r;
        canvasColors[canvasColorIndex] = canvasColorR == COLOR_PIXEL_ON.r || canvasColorR == COLOR_PIXEL_ON_SELECTED.r ? COLOR_PIXEL_ON : COLOR_PIXEL_OFF;
    }

    private void LoadTest()
    {
        infoText.text = "";
        test = tests[testIndex];
        Texture2D newCanvasTexture = test.canvasTexture;
        canvasColors = newCanvasTexture.GetPixels32();
        canvasTextureHeight = newCanvasTexture.height;
        canvasTextureWidth = newCanvasTexture.width;
        _ = canvasTexture.Reinitialize(canvasTextureWidth, canvasTextureHeight);
        GetComponent<Image>().material.mainTexture = canvasTexture;
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 sizeDelta = rectTransform.sizeDelta;
        float maxDimension = Mathf.Max(sizeDelta.x, sizeDelta.y);
        sizeDelta.x = canvasTextureHeight <= canvasTextureWidth ? maxDimension : maxDimension * canvasTextureWidth / canvasTextureHeight;
        sizeDelta.y = canvasTextureHeight >= canvasTextureWidth ? maxDimension : maxDimension * canvasTextureHeight / canvasTextureWidth;
        rectTransform.sizeDelta = sizeDelta;
        canvasColorIndex = canvasTextureX = canvasTextureY = programIndex = 0;
        returnCode = -1;
        if (breakpoints.Contains(programIndex))
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
        slotButtons[1].interactable = saveLevel.autoSave != null;
#if UNITY_EDITOR
        // Unlock author solution for debugging
        slotButtons[2].interactable = true;
#else
        slotButtons[2].interactable = saveLevel.starCount == 3;
#endif
        slotButtons[3].interactable = saveLevel.bestSave != null;
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
        ipf = Mathf.Floor(Mathf.Min(queuedInstructions, maxIpf));
        int instruction = 0;
        if (!playToggle.isOn || program == null || LevelEnded(ref instruction))
        {
            return;
        }

        UnSelectPixel();
        for (; instruction < ipf; instruction++)
        {
            canvasColorIndex = (canvasTextureY * canvasTextureWidth) + canvasTextureX;
            float canvasColorR = canvasColors[canvasColorIndex].r;
            ref BtmlAction action = ref (canvasColorR == COLOR_PIXEL_ON.r ? ref program[programIndex].blackAction : ref program[programIndex].whiteAction);
            canvasColors[canvasColorIndex] = action.writeColor;
            int newProgramIndex = action.gotoLine;
            if (newProgramIndex < 0)
            {
                Exit(-newProgramIndex - 1, ref instruction);
                continue;
            }

            switch (action.moveDirection)
            {
                case MoveDirection.Left:
                    if (--canvasTextureX == -1)
                    {
                        goto default;
                    }

                    break;
                case MoveDirection.Up:
                    if (++canvasTextureY == canvasTextureHeight)
                    {
                        goto default;
                    }

                    break;
                case MoveDirection.Right:
                    if (++canvasTextureX == canvasTextureWidth)
                    {
                        goto default;
                    }

                    break;
                case MoveDirection.Down:
                    if (--canvasTextureY == -1)
                    {
                        goto default;
                    }

                    break;
                case MoveDirection.None:
                default:
                    Exit(2, ref instruction);
                    continue;
            }

            programIndex = newProgramIndex;
            if (breakpoints.Contains(programIndex))
            {
                pauseToggle.isOn = true;
            }
        }

        if (instruction > 0)
        {
            codeEditor.MarkedLineIndex = programIndex;
            executedInstructions += instruction;
            queuedInstructions -= instruction;
            SelectPixel();
            canvasTexture.SetPixels32(canvasColors);
            canvasTexture.Apply();
        }

        elapsedTime += Time.unscaledDeltaTime;
        if (returnCode <= -1 && elapsedTime > UPDATE_INTERVAL && executedInstructions > 0f)
        {
            infoText.text = $"{executedInstructions / elapsedTime:F1} IPS";
            elapsedTime = executedInstructions = 0f;
        }
    }

    private void Exit(int newReturnCode, ref int instruction)
    {
        returnCode = newReturnCode;
        infoText.text = $"EXIT {returnCode}";
        instruction++;
        _ = LevelEnded(ref instruction);
        instruction--;
    }

    private bool LevelEnded(ref int instruction)
    {
        if (testIndex >= tests.Length)
        {
            return true;
        }

        if (returnCode <= -1 || instruction >= ipf)
        {
            return false;
        }

        if (returnCode == test.returnCode && ++testIndex < tests.Length)
        {
            LoadTest();
            instruction++;
            return false;
        }

        int instructionCount = program.Length;
        program = null;
        levelEndedMenuOverlay.SetActive(true);
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

            slotButtons[2].interactable = saveLevel.starCount == 3;
            slotButtons[3].interactable = true;
            SaveFunctions.LoadGame().levels[levelIndex] = saveLevel;
            SaveFunctions.SaveGame();
        }
        for (int starIndex = 0; starIndex < stars.Length; starIndex++)
        {
            stars[starIndex].SetActive(starIndex < starCount);
        }

        nextButton.gameObject.SetActive(levelIndex + 1 < saveLevels.Count && levelIndex + 1 < LEVEL_COUNT);
        nextButton.onClick.RemoveAllListeners();
        if (nextButton.gameObject.activeSelf)
        {
            nextButton.onClick.AddListener(() => LoadLevel(levelIndex + 1));
        }

        EventSystem.current.SetSelectedGameObject(nextButton.gameObject.activeSelf ? nextButton.gameObject : menuButton.gameObject);
        ipf = queuedInstructions = 0f;
        return true;
    }
}
