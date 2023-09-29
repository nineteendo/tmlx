using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

// TODO - Speed up syntax highlighter

public abstract class TokenIdentifier
{
    public string tokenType;
    public abstract int Match(string text, int startIndex);
}

[Serializable]
public struct CommentDelimiters
{
    public string PrefixPattern
    {
        readonly get => prefixPattern;
        set
        {
            prefixRegex = new Regex($@"\G({value})", RegexOptions.Compiled);
            prefixPattern = value;
        }
    }
    public string SuffixPattern
    {
        readonly get => suffixPattern;
        set
        {
            suffixRegex = new Regex(value, RegexOptions.Compiled);
            suffixPattern = value;
        }
    }

    public Regex prefixRegex;
    public Regex suffixRegex;

    [SerializeField] private string prefixPattern;
    [SerializeField] private string suffixPattern;
}

[Serializable]
public class CommentTokenIdentifier : TokenIdentifier
{
    public CommentDelimiters[] commentDelimitersArray;

    public override int Match(string text, int startIndex)
    {
        foreach (CommentDelimiters commentDelimiters in commentDelimitersArray)
        {
            Match match = commentDelimiters.prefixRegex.Match(text, startIndex);
            if (!match.Success)
            {
                continue;
            }

            match = commentDelimiters.suffixRegex.Match(text, startIndex + match.Length);
            return (!match.Success ? text.Length : match.Index + match.Length) - startIndex;
        }

        return -1;
    }
}

[Serializable]
public class RegexTokenIdentifier : TokenIdentifier
{
    public string Pattern
    {
        get => pattern;
        set
        {
            regex = new Regex($@"\G\b({value})\b", RegexOptions.Compiled);
            pattern = value;
        }
    }

    public Regex regex;

    [SerializeField] private string pattern;

    public override int Match(string text, int startIndex)
    {
        Match match = regex.Match(text, startIndex);
        return !match.Success ? -1 : match.Length;
    }
}


[CreateAssetMenu(fileName = "New Syntax Highlighter", menuName = "Btml/Syntax Highlighter")]
public class SyntaxHighlighter : ScriptableObject
{
    public CommentTokenIdentifier[] commentTokenIdentifiers = {
        new()
        {
            commentDelimitersArray = new CommentDelimiters[]
            {
                new() {
                    PrefixPattern = @"\/\/\/",
                    SuffixPattern = @"\n|\v"
                },
                new() {
                    PrefixPattern = @"\/\*\*(?!/)",
                    SuffixPattern = @"\*\/"
                }
            },
            tokenType = "comment.documentation"
        },
        new()
        {
            commentDelimitersArray = new CommentDelimiters[]
            {
                new() {
                    PrefixPattern = @"\/\/",
                    SuffixPattern = @"\n|\v"
                },
                new() {
                    PrefixPattern = @"\/\*",
                    SuffixPattern = @"\*\/"
                }
            },
            tokenType = "comment.normal"
        }
    };

    public RegexTokenIdentifier[] regexTokenIdentifiers = {
        new() {
            Pattern = @"black|white",
            tokenType = "identifier.constant"
        },
        new() {
            Pattern = @"down|left|right|up|write",
            tokenType = "identifier.function"
        },
        new() {
            Pattern = @"else|exit|goto|if|while",
            tokenType = "keyword.control"
        },
        new() {
            Pattern = @"\d+",
            tokenType = "literal.number"
        }
    };

    public Token[] Tokenize(string text)
    {
        List<Token> tokens = new();
        List<TokenIdentifier> tokenIdentifiers = new(commentTokenIdentifiers);
        tokenIdentifiers.AddRange(regexTokenIdentifiers);
        for (int startIndex = 0; startIndex < text.Length; startIndex++)
        {
            foreach (TokenIdentifier tokenIdentifier in tokenIdentifiers)
            {
                int length = tokenIdentifier.Match(text, startIndex);
                if (length == -1)
                {
                    continue;
                }

                tokens.Add(
                    new Token()
                    {
                        length = length,
                        startIndex = startIndex,
                        tokenType = tokenIdentifier.tokenType
                    }
                );
                startIndex += length - 1;
                break;
            }
        }

        return tokens.ToArray();
    }
}
