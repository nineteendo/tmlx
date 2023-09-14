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
    public HashSet<int> breakpoints = new();
    public Toggle breakpointTogglePrebab;
    public SyntaxHighlighter syntaxHighlighter;
    public TMP_BetterInputField inputField;
    public TMP_Text lineNumbersText;
    public TMP_Text highlightedText;

    public int MarkedLineIndex
    {
        get => markedLineIndex;
        set
        {
            update = update || value != markedLineIndex;
            markedLineIndex = value;
        }
    }

    private readonly List<Toggle> breakpointToggles = new();

    private bool update = true;
    private float heightPerChar;
    private float widthPerChar;
    private int markedEndIndex = -1;
    private int markedLineIndex = -1;
    private int markedStartIndex = -1;

    public void ToggleBreakpoint(int breakpointToggleIndex)
    {
        _ = breakpointToggles[breakpointToggleIndex].isOn
            ? breakpoints.Add(breakpointToggleIndex)
            : breakpoints.Remove(breakpointToggleIndex);
    }


    private void UpdateMarkedIndices(string text)
    {
        if (markedLineIndex < 0)
        {
            markedEndIndex = markedStartIndex = -1;
            return;
        }

        markedStartIndex = 0;
        for (int lineNumber = 0; lineNumber < markedLineIndex; lineNumber++)
        {
            int newMarkedStartIndex = text.IndexOf('\n', markedStartIndex);
            if (newMarkedStartIndex == -1)
            {
                markedEndIndex = markedStartIndex = -1;
                return;
            }

            markedStartIndex = newMarkedStartIndex + 1;
        }

        int newMarkedEndIndex = text.IndexOf('\n', markedStartIndex);
        markedEndIndex = newMarkedEndIndex == -1 ? text.Length - 1 : newMarkedEndIndex;
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
        if (update)
        {
            UpdateLineNumbers();
            UpdateSyntaxHighlighting();
            update = false;
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

    private void UpdateSyntaxHighlighting()
    {
        StringBuilder highlightedTextBuilder = new();
        Dictionary<string, Color32> tokenColoringDictionary = colorTheme.GetTokenColoringDictionary();
        int startIndex = 0;
        string text = inputField.textComponent.text;
        UpdateMarkedIndices(text);
        foreach (Token token in syntaxHighlighter.Tokenize(text))
        {
            if (!tokenColoringDictionary.ContainsKey(token.tokenType))
            {
                continue;
            }

            _ = highlightedTextBuilder.Append(HighlightSubString(text, startIndex, token.startIndex));
            _ = highlightedTextBuilder.Append($"<#{ColorUtility.ToHtmlStringRGB(tokenColoringDictionary[token.tokenType])}>");
            _ = highlightedTextBuilder.Append(HighlightSubString(text, token.startIndex, token.startIndex + token.length));
            _ = highlightedTextBuilder.Append($"</color>");
            startIndex = token.startIndex + token.length;
        }

        _ = highlightedTextBuilder.Append(HighlightSubString(text, startIndex, text.Length));
        highlightedText.text = highlightedTextBuilder.ToString();
    }

    private void Start()
    {
        inputField.onValueChanged.AddListener((_) => update = true);
        widthPerChar = inputField.textComponent.GetPreferredValues("0").x;
        heightPerChar = inputField.textComponent.GetPreferredValues("0").y;
    }


    private string HighlightSubString(string text, int startIndex, int endIndex)
    {
        string result;
        if (startIndex > markedStartIndex || markedStartIndex >= endIndex)
        {
            result = "";
        }
        else
        {
            result = text[startIndex..markedStartIndex] + "<mark=#ffff0033>";
            startIndex = markedStartIndex;
        }

        return startIndex > markedEndIndex || markedEndIndex >= endIndex
            ? result + text[startIndex..endIndex]
            : result + text[startIndex..markedEndIndex] + "</mark>" + text[markedEndIndex..endIndex];
    }
}
