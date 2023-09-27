using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public struct BtmlAction
{
    public Color32 writeColor;
    public BtmlDirection moveDirection;

    public int gotoLineIndex;
    public string gotoLabel;
}

public struct BtmlBranch
{
    public Color32 color;

    public int lineIndex;
}

public enum BtmlDirection
{
    N, north = 0,
    E, east = 1,
    S, south = 2,
    W, west = 3,
    none
}

public struct BtmlInstruction
{
    public BtmlAction blackAction;
    public BtmlAction whiteAction;

    public bool conditional;
}


public class BtmlBranchEqualityComparer : IEqualityComparer<BtmlBranch>
{
    public bool Equals(BtmlBranch x, BtmlBranch y)
    {
        return x.lineIndex == y.lineIndex && x.color.Equals(y.color);
    }

    public int GetHashCode(BtmlBranch obj)
    {
        unchecked  // Don't check overflow
        {
            int hash = 17;
            hash = (hash * 23) + obj.lineIndex.GetHashCode();
            hash = (hash * 23) + obj.color.GetHashCode();
            return hash;
        }
    }
}

public static class BtmlCompiler
{
    private static readonly string[] reservedWords = { ":", "E", "N", "S", "W", "black", "down", "east", "else", "exit", "goto", "if", "left", "move", "north", "repeat", "right", "south", "up", "west", "while", "white", "write" };

    public static bool Compile(string text, out BtmlInstruction[] precomputedInstructions, out string error)
    {
        if (text == null)
        {
            error = "text is null";
            precomputedInstructions = null;
            return false;
        }

        // Process
        if (!Process(text, out BtmlInstruction[] processedInstructions, out Dictionary<string, int> labels, out string processError))
        {
            error = processError;
            precomputedInstructions = null;
            return false;
        }

        // Expand
        BtmlInstruction[] expandedInstructions = new BtmlInstruction[processedInstructions.Length];
        for (int processedInstructionIndex = 0; processedInstructionIndex < processedInstructions.Length; processedInstructionIndex++)
        {
            BtmlInstruction processedInstruction = processedInstructions[processedInstructionIndex];
            ref BtmlAction whiteAction = ref processedInstruction.whiteAction;
            ref BtmlAction blackAction = ref processedInstruction.blackAction;
            if (whiteAction.writeColor.a < 0xff)
            {
                whiteAction.writeColor = BtmlRuntime.COLOR_PIXEL_OFF;
            }

            if (blackAction.writeColor.a < 0xff)
            {
                blackAction.writeColor = BtmlRuntime.COLOR_PIXEL_ON;
            }

            if (!Resolve(processedInstructionIndex, labels, ref whiteAction, out string resolveError) || !Resolve(processedInstructionIndex, labels, ref blackAction, out resolveError))
            {
                error = resolveError;
                precomputedInstructions = null;
                return false;
            }

            expandedInstructions[processedInstructionIndex] = processedInstruction;
        }

        // Precompute
        precomputedInstructions = (BtmlInstruction[])expandedInstructions.Clone();
        for (int precomputedInstructionIndex = 0; precomputedInstructionIndex < precomputedInstructions.Length; precomputedInstructionIndex++)
        {
            ref BtmlInstruction precomputedInstruction = ref precomputedInstructions[precomputedInstructionIndex];
            HashSet<BtmlBranch> whiteVisitedBranches = new(new BtmlBranchEqualityComparer());
            HashSet<BtmlBranch> blackVisitedBranches = new(new BtmlBranchEqualityComparer());
            _ = whiteVisitedBranches.Add(new BtmlBranch() { color = BtmlRuntime.COLOR_PIXEL_OFF, lineIndex = precomputedInstructionIndex });
            _ = blackVisitedBranches.Add(new BtmlBranch() { color = BtmlRuntime.COLOR_PIXEL_ON, lineIndex = precomputedInstructionIndex });
            if (
                !Precompute(precomputedInstructions, whiteVisitedBranches, ref precomputedInstruction.whiteAction, out string precomputeError) ||
                !Precompute(precomputedInstructions, blackVisitedBranches, ref precomputedInstruction.blackAction, out precomputeError)
            )
            {
                error = precomputeError;
                return false;
            }
        }

        error = null;
        return true;
    }


    private static List<string> Tokenize(int lineIndex, string line, ref int blockCommentStartIndex)
    {
        // Remove single-line comments (//) and block comments (/* */)
        List<string> tokens = new();
        StringBuilder token = new();
        string[] subLines = line.Split('\v');
        for (int subLineIndex = 0; subLineIndex < subLines.Length; subLineIndex++)
        {
            string subLine = subLines[subLineIndex];
            for (int charIndex = 0; charIndex < subLine.Length; charIndex++)
            {
                if (charIndex + 1 < subLine.Length && blockCommentStartIndex <= -1 && subLine[charIndex] == '/' && subLine[charIndex + 1] == '/')
                {
                    break;  // Re-use token line break token splitter
                }

                if (charIndex + 1 < subLine.Length && blockCommentStartIndex <= -1 && subLine[charIndex] == '/' && subLine[charIndex + 1] == '*')
                {
                    charIndex++; // Skip the '*' character
                    blockCommentStartIndex = lineIndex;
                }
                else if (charIndex + 1 < subLine.Length && blockCommentStartIndex > -1 && subLine[charIndex] == '*' && subLine[charIndex + 1] == '/')
                {
                    if (token.Length > 0)
                    {
                        tokens.Add(token.ToString());
                        _ = token.Clear();
                    }

                    charIndex++; // Skip the '/' character
                    blockCommentStartIndex = -1;
                }
                else if (blockCommentStartIndex > -1)
                {
                    continue;
                }
                else if (subLine[charIndex] == ':')
                {
                    if (token.Length > 0)
                    {
                        tokens.Add(token.ToString());
                        _ = token.Clear();
                    }

                    tokens.Add(subLine[charIndex].ToString());
                }
                else if (subLine[charIndex] is not ' ' and not '\t')
                {
                    _ = token.Append(subLine[charIndex]);
                }
                else if (token.Length > 0)
                {
                    tokens.Add(token.ToString());
                    _ = token.Clear();
                }
            }

            if (token.Length > 0)
            {
                tokens.Add(token.ToString());
                _ = token.Clear();
            }
        }

        return tokens;
    }


    private static bool ParseAction(string[] lines, int lineIndex, List<string> tokens, ref int tokenIndex, out BtmlAction action, out string error)
    {
        action = new BtmlAction
        {
            moveDirection = BtmlDirection.none,
            gotoLineIndex = lineIndex + 1 < lines.Length ? lineIndex + 1 : -1
        };

        // Parse write
        if (tokenIndex < tokens.Count && tokens[tokenIndex] == "write")
        {
            if (!ParseColor(lineIndex, tokens, ref tokenIndex, out action.writeColor, out string colorError))
            {
                error = colorError;
                return false;
            }

            tokenIndex++;
        }

        // Parse move
        if (tokenIndex < tokens.Count && tokens[tokenIndex] == "move")
        {
            if (++tokenIndex >= tokens.Count)
            {
                error = $"Line {lineIndex + 1}: direction is missing";
                return false;
            }

            if (!Enum.TryParse(tokens[tokenIndex], out action.moveDirection) || action.moveDirection == BtmlDirection.none)  // None is for internal use only
            {
                error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: '{tokens[tokenIndex]}' is not a valid direction";
                return false;
            }

            tokenIndex++;
        }

        // Parse gotoLine
        if (tokenIndex < tokens.Count && tokens[tokenIndex] == "repeat")
        {
            tokenIndex++;
            action.gotoLineIndex = lineIndex;
        }
        else if (tokenIndex < tokens.Count && tokens[tokenIndex] == "goto")
        {
            if (++tokenIndex >= tokens.Count)
            {
                error = $"Line {lineIndex + 1}: label is missing";
                return false;
            }

            if (reservedWords.Contains(tokens[tokenIndex]))
            {
                error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: '{tokens[tokenIndex]}' is reserved from use as a label";
                return false;
            }

            action.gotoLabel = tokens[tokenIndex++];
        }
        else if (tokenIndex < tokens.Count && tokens[tokenIndex] == "exit")
        {
            if (++tokenIndex >= tokens.Count || !int.TryParse(tokens[tokenIndex], out action.gotoLineIndex))
            {
                action.gotoLineIndex = -1;
            }
            else if (action.gotoLineIndex >= 0)
            {
                action.gotoLineIndex *= -1;
                action.gotoLineIndex--;
                tokenIndex++;
            }
            else
            {
                error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: '{tokens[tokenIndex]}' is not a valid exit status";
                return false;
            }
        }

        error = null;
        return true;
    }

    private static bool ParseColor(int lineIndex, List<string> tokens, ref int tokenIndex, out Color32 color, out string error)
    {
        if (++tokenIndex >= tokens.Count)
        {
            color = Color.clear;
            error = $"Line {lineIndex + 1}: color is missing";
            return false;
        }

        bool number = int.TryParse(tokens[tokenIndex], out int bit);
        if (number ? bit == 0 : tokens[tokenIndex] == "white")
        {
            color = BtmlRuntime.COLOR_PIXEL_OFF;
            error = null;
            return true;
        }

        if (number ? bit == 1 : tokens[tokenIndex] == "black")
        {
            color = BtmlRuntime.COLOR_PIXEL_ON;
            error = null;
            return true;
        }

        color = Color.clear;
        error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: '{tokens[tokenIndex]}' is not a valid color";
        return false;
    }

    private static bool Precompute(BtmlInstruction[] precomputedInstructions, HashSet<BtmlBranch> visitedBranches, ref BtmlAction action, out string error)
    {
        if (action.moveDirection != BtmlDirection.none)
        {
            error = null;
            return true;
        };

        if (action.gotoLineIndex < 0)
        {
            action.moveDirection = BtmlDirection.east;
            error = null;
            return true;
        }

        BtmlBranch branch = new()
        {
            color = action.writeColor,
            lineIndex = action.gotoLineIndex
        };
        if (visitedBranches.Contains(branch))
        {
            error = $"Line {action.gotoLineIndex + 1}: found infinite loop without moving";
            return false;
        }

        _ = visitedBranches.Add(branch);
        ref BtmlInstruction precomputedInstruction = ref precomputedInstructions[action.gotoLineIndex];
        ref BtmlAction newAction = ref (action.writeColor.Equals(BtmlRuntime.COLOR_PIXEL_OFF) ? ref precomputedInstruction.whiteAction : ref precomputedInstruction.blackAction);
        if (!Precompute(precomputedInstructions, visitedBranches, ref newAction, out error))
        {
            return false;
        }

        action = newAction;
        return true;
    }

    private static bool Process(string text, out BtmlInstruction[] instructions, out Dictionary<string, int> labels, out string error)
    {
        List<BtmlInstruction> instructionList = new();
        labels = new Dictionary<string, int>();
        int blockCommentStartIndex = -1;
        string[] lines = text.Split('\n');
        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            BtmlInstruction instruction = new();
            List<string> tokens = Tokenize(lineIndex, lines[lineIndex], ref blockCommentStartIndex);
            if (tokens.Count == 0)
            {
                instructions = null;
                error = $"Line {lineIndex + 1}: found empty line, use shift enter if an empty line was intended";
                return false;
            }

            int tokenIndex = 0;
            if (tokenIndex < tokens.Count && tokens[tokenIndex] == ":")
            {
                instructions = null;
                error = $"Line {lineIndex + 1}: label is missing before colon";
                return false;
            }

            if (tokenIndex + 1 < tokens.Count && tokens[tokenIndex + 1] == ":")
            {
                if (reservedWords.Contains(tokens[tokenIndex]))
                {
                    instructions = null;
                    error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: '{tokens[tokenIndex]}' is reserved from use as a label";
                    return false;
                }

                if (labels.ContainsKey(tokens[tokenIndex]))
                {
                    instructions = null;
                    error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: label '{tokens[tokenIndex]}' is already defined on line {labels[tokens[tokenIndex]] + 1}";
                    return false;
                }

                labels.Add(tokens[tokenIndex++], lineIndex);
                if (++tokenIndex >= tokens.Count)
                {
                    instructions = null;
                    error = $"Line {lineIndex + 1}: instruction is missing after label, use shift enter if a line break was intended";
                    return false;
                }
            }

            int oldTokenIndex = tokenIndex;
            if (tokenIndex < tokens.Count && tokens[tokenIndex] == "if")
            {
                if (!ParseColor(lineIndex, tokens, ref tokenIndex, out Color32 condition, out string colorError))
                {
                    instructions = null;
                    error = colorError;
                    return false;
                }

                if (++tokenIndex >= tokens.Count)
                {
                    instructions = null;
                    error = $"Line {lineIndex + 1}: 'if' action is missing";
                    return false;
                }

                oldTokenIndex = tokenIndex;
                if (!ParseAction(lines, lineIndex, tokens, ref tokenIndex, out BtmlAction consequentAction, out string actionError))
                {
                    instructions = null;
                    error = actionError;
                    return false;
                }

                if (tokenIndex == oldTokenIndex)
                {
                    instructions = null;
                    error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: found '{tokens[tokenIndex]}' before 'if' action";
                    return false;
                }

                BtmlAction alternativeAction;
                if (tokenIndex >= tokens.Count || tokens[tokenIndex] != "else")
                {
                    alternativeAction = new BtmlAction
                    {
                        moveDirection = BtmlDirection.none,
                        gotoLineIndex = lineIndex + 1 < lines.Length ? lineIndex + 1 : -1
                    };
                }
                else if (++tokenIndex >= tokens.Count)
                {
                    instructions = null;
                    error = $"Line {lineIndex + 1}: 'else' action is missing";
                    return false;
                }
                else
                {
                    oldTokenIndex = tokenIndex;
                    if (!ParseAction(lines, lineIndex, tokens, ref tokenIndex, out alternativeAction, out actionError))
                    {
                        instructions = null;
                        error = actionError;
                        return false;
                    }

                    if (tokenIndex == oldTokenIndex)
                    {
                        instructions = null;
                        error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: found '{tokens[tokenIndex]}' before 'else' action";
                        return false;
                    }
                }

                instruction.blackAction = condition.Equals(BtmlRuntime.COLOR_PIXEL_ON) ? consequentAction : alternativeAction;
                instruction.whiteAction = condition.Equals(BtmlRuntime.COLOR_PIXEL_OFF) ? consequentAction : alternativeAction;
                instruction.conditional = true;
            }
            else if (ParseAction(lines, lineIndex, tokens, ref tokenIndex, out BtmlAction action, out string actionError))
            {
                instruction.blackAction = instruction.whiteAction = action;
            }
            else
            {
                instructions = null;
                error = actionError;
                return false;
            }

            // Check end
            if (tokenIndex == oldTokenIndex)
            {
                instructions = null;
                error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: '{tokens[tokenIndex]}' is not a valid instruction";
                return false;
            }

            if (tokenIndex < tokens.Count)
            {
                instructions = null;
                error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: '{tokens[tokenIndex]}' should be on the next line";
                return false;
            }

            instructionList.Add(instruction);
        }

        if (blockCommentStartIndex > -1)
        {
            instructions = null;
            error = $"Line {blockCommentStartIndex + 1}: block comment is not terminated";
            return false;
        }

        instructions = instructionList.ToArray();
        error = null;
        return true;
    }

    private static bool Resolve(int processedInstructionIndex, Dictionary<string, int> labels, ref BtmlAction action, out string error)
    {
        if (action.gotoLabel == null)
        {
            error = null;
            return true;
        }

        if (!labels.ContainsKey(action.gotoLabel))
        {
            error = $"Line {processedInstructionIndex + 1}: label '{action.gotoLabel}' is not defined";
            return false;
        }

        action.gotoLineIndex = labels[action.gotoLabel];
        action.gotoLabel = null;
        error = null;
        return true;
    }
}
