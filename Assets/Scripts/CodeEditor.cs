using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// TODO - Destroy invisible breakpoint toggles

public struct Token
{
    public int length;
    public int startIndex;
    public string tokenType;
}

public class CodeEditor : MonoBehaviour
{
    public ColorTheme colorTheme;
    public Color32[] MarkedLines
    {
        get => markedLines == null ? null : (Color32[])markedLines.Clone();
        set
        {
            update = true;
            markedLines = value == null ? value : (Color32[])value.Clone();
        }
    }
    public HashSet<int> breakpoints = new();
    public SyntaxHighlighter syntaxHighlighter;
    public Toggle breakpointTogglePrebab;
    public TMP_BetterInputField inputField;
    public TMP_Text highlightedText;
    public TMP_Text lineNumbersText;

    public int MarkedLineIndex
    {
        get => markedLineIndex;
        set
        {
            update = update || value != markedLineIndex;
            markedLineIndex = value;
        }
    }

    private Color32[] markedLines;
    private readonly List<Toggle> breakpointToggles = new();

    private bool fullUpdate = true;
    private bool update = true;
    private float heightPerChar;
    private float widthPerChar;
    private int markedLineIndex = -1;
    private string[] lines;

    public void ToggleBreakpoint(int breakpointToggleIndex)
    {
        _ = breakpointToggles[breakpointToggleIndex].isOn
            ? breakpoints.Add(breakpointToggleIndex)
            : breakpoints.Remove(breakpointToggleIndex);
    }

    private void SetBreakpointPosition(int breakpointToggleIndex, int lineIndex)
    {
        if (breakpointToggleIndex > breakpointToggles.Count)
        {
            return;
        }

        Toggle breakpointToggle;
        if (breakpointToggleIndex < breakpointToggles.Count)
        {
            breakpointToggle = breakpointToggles[breakpointToggleIndex];
            if (breakpointToggle == null)
            {
                return;
            }
        }
        else
        {
            breakpointToggle = Instantiate(breakpointTogglePrebab, lineNumbersText.transform);
            breakpointToggle.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(widthPerChar, widthPerChar);
            breakpointToggle.onValueChanged.AddListener((_) => ToggleBreakpoint(breakpointToggleIndex));
            breakpointToggles.Add(breakpointToggle);
        }

        breakpointToggle.gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(-widthPerChar, -heightPerChar * (lineIndex + .5f));
    }

    private void LateUpdate()
    {
        if (fullUpdate)
        {
            UpdateLineNumbers();
            UpdateLines();
        }

        if (fullUpdate || update)
        {
            UpdateMarkedText();
            fullUpdate = update = false;
        }
    }

    private void UpdateLineNumbers()
    {
        StringBuilder lineNumberBuilder = new();
        string text = inputField.textComponent.text;

        // Update width
        float maxChars = Mathf.Ceil(Mathf.Log10(text.Count(c => c == '\n') + 2));
        Vector2 offsetMin = inputField.textViewport.offsetMin;
        offsetMin.x = (maxChars + 3f) * widthPerChar;
        inputField.textViewport.offsetMin = offsetMin;
        Vector2 anchoredPosition = lineNumbersText.rectTransform.anchoredPosition;
        anchoredPosition.x = -(maxChars + 2f) * widthPerChar / 2f;
        lineNumbersText.rectTransform.anchoredPosition = anchoredPosition;
        Vector2 sizeDelta = lineNumbersText.rectTransform.sizeDelta;
        sizeDelta.x = maxChars * widthPerChar;
        lineNumbersText.rectTransform.sizeDelta = sizeDelta;

        // Update breakpoints and text
        int lineCount = inputField.textComponent.textInfo.lineCount;
        TMP_LineInfo[] lineInfo = inputField.textComponent.textInfo.lineInfo;
        int lineIndex = 0;
        int lineNumber = 1;
        SetBreakpointPosition(lineNumber - 1, lineIndex);
        _ = lineNumberBuilder.Append(lineNumber);
        for (; lineIndex < lineCount - 1; lineIndex++)
        {
            _ = lineNumberBuilder.Append('\n');
            if (!text.Substring(lineInfo[lineIndex].firstCharacterIndex, lineInfo[lineIndex].characterCount).EndsWith("\n"))
            {
                continue;
            }

            SetBreakpointPosition(lineNumber++, lineIndex + 1);
            _ = lineNumberBuilder.Append(lineNumber);
        }

        lineNumbersText.text = lineNumberBuilder.ToString();
        for (int breakpointToggleIndex = breakpointToggles.Count - 1; breakpointToggleIndex >= lineNumber; breakpointToggleIndex--)
        {
            _ = breakpoints.Remove(breakpointToggleIndex);
            Toggle breakpointToggle = breakpointToggles[breakpointToggleIndex];
            breakpointToggles.RemoveAt(breakpointToggleIndex);
            Destroy(breakpointToggle.gameObject);
        }
    }

    private void UpdateLines()
    {
        StringBuilder coloredTextBuilder = new();
        Dictionary<string, Color32> tokenColoringDictionary = colorTheme.GetTokenColoringDictionary();
        int startIndex = 0;
        string text = inputField.textComponent.text;
        foreach (Token token in syntaxHighlighter.Tokenize(text))
        {
            if (!tokenColoringDictionary.ContainsKey(token.tokenType))
            {
                continue;
            }

            _ = coloredTextBuilder.Append(text[startIndex..token.startIndex]);
            _ = coloredTextBuilder.Append($"<#{ColorUtility.ToHtmlStringRGB(tokenColoringDictionary[token.tokenType])}>");
            _ = coloredTextBuilder.Append(text.Substring(token.startIndex, token.length));
            _ = coloredTextBuilder.Append($"</color>");
            startIndex = token.startIndex + token.length;
        }

        _ = coloredTextBuilder.Append(text[startIndex..]);
        lines = coloredTextBuilder.ToString().Split('\n');
    }

    private void UpdateMarkedText()
    {
        List<string> markedTextList = new();
        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            string line = lines[lineIndex];
            if (markedLines != null && lineIndex < markedLines.Length)
            {
                markedTextList.Add($"<mark=#{ColorUtility.ToHtmlStringRGBA(markedLines[lineIndex])}>{line}</mark>");
            }
            else if (lineIndex != markedLineIndex)
            {
                markedTextList.Add(line);
            }
            else
            {
                markedTextList.Add($"<mark=#ffff0033>{line}</mark>");
            }
        }

        highlightedText.text = string.Join('\n', markedTextList);
    }

    private void Start()
    {
        inputField.onValueChanged.AddListener((_) => fullUpdate = true);
        widthPerChar = inputField.textComponent.GetPreferredValues("0").x;
        heightPerChar = inputField.textComponent.GetPreferredValues("0").y;
    }
}
