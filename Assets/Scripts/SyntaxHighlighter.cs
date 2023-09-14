using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class TokenIdentifier
{
    public string tokenType;
    public abstract int Match(string text, int startIndex);
}

[Serializable]
public struct CommentDelimiters
{
    public string commentPrefix;
    public string commentSuffix;
}

[Serializable]
public class CommentTokenIdentifier : TokenIdentifier
{
    public CommentDelimiters[] commentDelimitersArray;

    public override int Match(string text, int startIndex)
    {
        foreach (CommentDelimiters commentDelimiters in commentDelimitersArray)
        {
            string commentPrefix = commentDelimiters.commentPrefix;
            if (text.Length - startIndex < commentPrefix.Length || text.Substring(startIndex, commentPrefix.Length) != commentPrefix)
            {
                continue;
            }

            int commentEndIndex;
            string commentSuffix = commentDelimiters.commentSuffix;
            commentEndIndex = commentSuffix != ""
                ? text.IndexOf(commentSuffix, startIndex + commentPrefix.Length)
                : text.IndexOfAny(new char[] { '\n', '\v' }, startIndex + commentPrefix.Length);

            return commentEndIndex != -1 ? commentEndIndex + (commentSuffix != "" ? commentSuffix.Length : 1) - startIndex : text.Length - startIndex;
        }

        return -1;
    }
}

[Serializable]
public class RegexTokenIdentifier : TokenIdentifier
{
    public string regex;

    public override int Match(string text, int startIndex)
    {
        Match match = new Regex($@"\G\b({regex})\b").Match(text, startIndex);
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
                    commentPrefix = "///"
                },
                new() {
                    commentPrefix = "/**",
                    commentSuffix = "*/"
                }
            },
            tokenType = "comment.documentation"
        },
        new()
        {
            commentDelimitersArray = new CommentDelimiters[]
            {
                new() {
                    commentPrefix = "//"
                },
                new() {
                    commentPrefix = "/*",
                    commentSuffix = "*/"
                }
            },
            tokenType = "comment.normal"
        }
    };

    public RegexTokenIdentifier[] regexTokenIdentifiers = {
        new() {
            regex = "Black|Down|Left|Right|Up|White",
            tokenType = "identifier.constant"
        },
        new() {
            regex = "move|write",
            tokenType = "identifier.function"
        },
        new() {
            regex = "else|exit|goto|if|label|repeat",
            tokenType = "keyword.control"
        },
        new() {
            regex = "\\d+",
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
