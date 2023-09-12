using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public abstract class TokenIdentifier
{
    public string tokenType;
    public abstract int Match(string text, int offset);
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

    public override int Match(string text, int offset)
    {
        foreach (CommentDelimiters commentDelimiters in commentDelimitersArray)
        {
            string commentPrefix = commentDelimiters.commentPrefix;
            if (text.Length - offset < commentPrefix.Length || text.Substring(offset, commentPrefix.Length) != commentPrefix)
                continue;

            int commentEndIndex;
            string commentSuffix = commentDelimiters.commentSuffix;
            if (commentSuffix != "")
                commentEndIndex = text.IndexOf(commentSuffix, offset + commentPrefix.Length);
            else  // Inline comment
                commentEndIndex = text.IndexOfAny(new char[] { '\n', '\v' }, offset + commentPrefix.Length);

            if (commentEndIndex != -1)
                return commentEndIndex + (commentSuffix != "" ? commentSuffix.Length : 1) - offset;

            return text.Length - offset;
        }

        return 0;
    }
}

[Serializable]
public class RegexTokenIdentifier : TokenIdentifier
{
    public string regex;

    public override int Match(string text, int offset)
    {
        Match match = new Regex($@"\G\b({regex})\b").Match(text, offset);
        if (!match.Success)
            return 0;

        return match.Length;
    }
}


[CreateAssetMenu(fileName = "New Syntax Highlighter", menuName = "Btml/Syntax Highlighter")]
public class SyntaxHighlighter : ScriptableObject
{
    public CommentTokenIdentifier[] commentTokenIdentifiers = {
        new CommentTokenIdentifier()
        {
            commentDelimitersArray = new CommentDelimiters[]
            {
                new CommentDelimiters() {
                    commentPrefix = "///"
                },
                new CommentDelimiters() {
                    commentPrefix = "/**",
                    commentSuffix = "*/"
                }
            },
            tokenType = "comment.documentation"
        },
        new CommentTokenIdentifier()
        {
            commentDelimitersArray = new CommentDelimiters[]
            {
                new CommentDelimiters() {
                    commentPrefix = "//"
                },
                new CommentDelimiters() {
                    commentPrefix = "/*",
                    commentSuffix = "*/"
                }
            },
            tokenType = "comment.normal"
        }
    };

    public RegexTokenIdentifier[] regexTokenIdentifiers = {
        new RegexTokenIdentifier() {
            regex = "Black|Down|Left|Right|Up|White",
            tokenType = "identifier.constant"
        },
        new RegexTokenIdentifier() {
            regex = "move|write",
            tokenType = "identifier.function"
        },
        new RegexTokenIdentifier() {
            regex = "else|exit|goto|if|label|repeat",
            tokenType = "keyword.control"
        },
        new RegexTokenIdentifier() {
            regex = "\\d+",
            tokenType = "literal.number"
        }
    };

    public Token[] Tokenize(string text)
    {
        List<Token> tokens = new List<Token>();
        List<TokenIdentifier> tokenIdentifiers = new List<TokenIdentifier>(commentTokenIdentifiers);
        tokenIdentifiers.AddRange(regexTokenIdentifiers);
        for (int offset = 0; offset < text.Length; offset++)
            foreach (TokenIdentifier tokenIdentifier in tokenIdentifiers)
            {
                int length = tokenIdentifier.Match(text, offset);
                if (length == 0)
                    continue;

                tokens.Add(
                    new Token()
                    {
                        length = length,
                        startIndex = offset,
                        tokenType = tokenIdentifier.tokenType
                    }
                );
                offset += length - 1;
                break;
            }

        return tokens.ToArray();
    }
}
