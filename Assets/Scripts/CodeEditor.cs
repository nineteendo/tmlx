using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

public struct Token
{
    public int length;
    public int startIndex;
    public string tokenType;
}

public class CodeEditor : MonoBehaviour
{
    public ColorTheme colorTheme;
    public SyntaxHighlighter syntaxHighlighter;
    public TMP_BetterInputField inputField;
    public TMP_Text lineNumbersText;
    public TMP_Text highlightedText;

    bool update = true;

    void LateUpdate()
    {
        if (update)
        {
            UpdateLineNumbers();
            UpdateSyntaxHighlighting();
            update = false;
        }
    }

    void UpdateLineNumbers()
    {
        StringBuilder lineNumberBuilder = new();
        string text = inputField.textComponent.text;

        // Update width
        float maxChars = Mathf.Ceil(Mathf.Log10(text.Count(c => c == '\n') + 2));
        float widthPerChar = inputField.textComponent.GetPreferredValues("0").x;
        float heightPerChar = inputField.textComponent.GetPreferredValues("0").y;
        Vector2 offsetMin = inputField.textViewport.offsetMin;
        offsetMin.x = (maxChars + 2f) * widthPerChar;
        inputField.textViewport.offsetMin = offsetMin;
        Vector2 anchoredPosition = lineNumbersText.rectTransform.anchoredPosition;
        anchoredPosition.x = -(maxChars + 2f) * widthPerChar / 2f;
        lineNumbersText.rectTransform.anchoredPosition = anchoredPosition;
        Vector2 sizeDelta = lineNumbersText.rectTransform.sizeDelta;
        sizeDelta.x = maxChars * widthPerChar;
        lineNumbersText.rectTransform.sizeDelta = sizeDelta;

        // Update text
        int lineCount = inputField.textComponent.textInfo.lineCount;
        TMP_LineInfo[] lineInfo = inputField.textComponent.textInfo.lineInfo;
        int lineNumber = 1;
        lineNumberBuilder.Append(lineNumber);
        for (int lineIndex = 0; lineIndex < lineCount - 1; lineIndex++)
        {
            lineNumberBuilder.Append('\n');
            if (!text.Substring(lineInfo[lineIndex].firstCharacterIndex, lineInfo[lineIndex].characterCount).EndsWith("\n"))
                continue;

            lineNumberBuilder.Append(++lineNumber);
        }

        lineNumbersText.text = lineNumberBuilder.ToString();
    }

    void UpdateSyntaxHighlighting()
    {
        StringBuilder highlightedTextBuilder = new();
        Dictionary<string, Color32> tokenColoringDictionary = colorTheme.GetTokenColoringDictionary();
        int offset = 0;
        string text = inputField.textComponent.text;
        foreach (Token token in syntaxHighlighter.Tokenize(text))
        {
            if (!tokenColoringDictionary.ContainsKey(token.tokenType))
                continue;
    
            int startIndex = token.startIndex;
            highlightedTextBuilder.Append(text.Substring(offset, startIndex - offset));
            highlightedTextBuilder.Append($"<#{ColorUtility.ToHtmlStringRGB(tokenColoringDictionary[token.tokenType])}>");
            highlightedTextBuilder.Append(text.Substring(startIndex, token.length));
            highlightedTextBuilder.Append($"</color>");
            offset = startIndex + token.length;
        }

        highlightedTextBuilder.Append(text.Substring(offset, text.Length - offset));
        highlightedText.text = highlightedTextBuilder.ToString();
    }

    void Start()
    {
        inputField.onValueChanged.AddListener((_) => update = true);
    }
}
