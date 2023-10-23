using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public struct TmlxAction
{
    public Color32 writeColor;
    public TmlxDirection moveDirection;

    public int gotoLineIndex;
    public string gotoLabel;
}

public struct TmlxBranch
{
    public Color32 color;

    public int lineIndex;
}

public enum TmlxDirection
{
    nowhere,
    up,
    down,
    left,
    right
}

public enum TmlxInstructionType
{
    nothing,
    unconditional,
    conditional
}

public struct TmlxInstruction
{
    public TmlxAction colorAction;
    public TmlxAction whiteAction;
    public TmlxInstructionType instructionType;
}


public class TmlxBranchEqualityComparer : IEqualityComparer<TmlxBranch>
{
    public bool Equals(TmlxBranch x, TmlxBranch y)
    {
        return x.lineIndex == y.lineIndex && x.color.Equals(y.color);
    }

    public int GetHashCode(TmlxBranch obj)
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

public static class TmlxCompiler
{
    private static readonly Dictionary<int, Color32> INT_TO_COLOR_OR_CONDITION_DICTIONARY = new() {
        { 0, TmlxRuntime.COLOR_PIXEL_WHITE },
        { 1, TmlxRuntime.COLOR_PIXEL_BLACK }
    };
    private static readonly Dictionary<string, Color32> STRING_TO_COLOR_DICTIONARY = new() {
        { "black",   TmlxRuntime.COLOR_PIXEL_BLACK   },
        { "blue",    TmlxRuntime.COLOR_PIXEL_BLUE    },
        { "cyan",    TmlxRuntime.COLOR_PIXEL_CYAN    },
        { "green",   TmlxRuntime.COLOR_PIXEL_GREEN   },
        { "magenta", TmlxRuntime.COLOR_PIXEL_MAGENTA },
        { "red",     TmlxRuntime.COLOR_PIXEL_RED     },
        { "white",   TmlxRuntime.COLOR_PIXEL_WHITE   },
        { "yellow",  TmlxRuntime.COLOR_PIXEL_YELLOW  }
    };
    private static readonly Dictionary<string, Color32> STRING_TO_CONDITION_DICTIONARY = new() {
        { "white", TmlxRuntime.COLOR_PIXEL_WHITE },
        { "color", TmlxRuntime.COLOR_PIXEL_BLACK }
    };
    private static readonly HashSet<string> reservedWords = new() { ":", "black", "blue", "color", "cyan", "down", "else", "exit", "goto", "green", "if", "left", "magenta", "nowhere", "red", "right", "up", "while", "white", "write", "yellow" };

    public static bool Compile(string text, bool optimised, out TmlxInstruction[] instructions, out int instructionCount, out string error)
    {
        if (text == null)
        {
            error = "text is null";
            instructions = null;
            instructionCount = 0;
            return false;
        }

        // Process
        if (!Process(text, out TmlxInstruction[] processedInstructions, out Dictionary<string, int> labelsDictionary, out instructionCount, out string processError))
        {
            error = processError;
            instructions = null;
            return false;
        }

        // Expand
        TmlxInstruction[] expandedInstructions = (TmlxInstruction[])processedInstructions.Clone();
        for (int expandedInstructionIndex = 0; expandedInstructionIndex < expandedInstructions.Length; expandedInstructionIndex++)
        {
            ref TmlxInstruction expandedInstruction = ref expandedInstructions[expandedInstructionIndex];
            ref TmlxAction whiteAction = ref expandedInstruction.whiteAction;
            ref TmlxAction colorAction = ref expandedInstruction.colorAction;
            if (!Resolve(expandedInstructionIndex, labelsDictionary, ref whiteAction, out string resolveError) || !Resolve(expandedInstructionIndex, labelsDictionary, ref colorAction, out resolveError))
            {
                error = resolveError;
                instructions = null;
                return false;
            }
        }

        // Pre-optimise
        HashSet<TmlxBranch> visitedBranches = new(new TmlxBranchEqualityComparer());
        TmlxInstruction[] preoptimisedInstructions = (TmlxInstruction[])expandedInstructions.Clone();
        for (int preoptimisedInstructionIndex = 0; preoptimisedInstructionIndex < preoptimisedInstructions.Length; preoptimisedInstructionIndex++)
        {
            ref TmlxInstruction preoptimisedInstruction = ref preoptimisedInstructions[preoptimisedInstructionIndex];
            Preoptimise(preoptimisedInstructions, visitedBranches, ref preoptimisedInstruction.whiteAction, TmlxRuntime.COLOR_PIXEL_WHITE);
            Preoptimise(preoptimisedInstructions, visitedBranches, ref preoptimisedInstruction.colorAction, TmlxRuntime.COLOR_PIXEL_BLACK);
        }

        instructions = (TmlxInstruction[])preoptimisedInstructions.Clone();
        if (!optimised)
        {
            error = null;
            return true;
        }

        // Optimise
        visitedBranches.Clear();
        for (int instructionIndex = 0; instructionIndex < instructions.Length; instructionIndex++)
        {
            ref TmlxInstruction optimisedInstruction = ref instructions[instructionIndex];
            if (optimisedInstruction.instructionType != TmlxInstructionType.nothing)
            {
                _ = Precompute(instructions, visitedBranches, ref optimisedInstruction.whiteAction, TmlxRuntime.COLOR_PIXEL_WHITE);
                _ = Precompute(instructions, visitedBranches, ref optimisedInstruction.colorAction, TmlxRuntime.COLOR_PIXEL_BLACK);
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


    private static bool ParseAction(string[] lines, int lineIndex, List<string> tokens, ref int tokenIndex, bool loop, out TmlxAction action, out string error)
    {
        action = new TmlxAction
        {
            gotoLineIndex = loop
                ? lineIndex
                : lineIndex + 1 < lines.Length
                    ? lineIndex + 1
                    : -1
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

        // Parse up, down, left or right
        if (tokenIndex < tokens.Count && Enum.TryParse(tokens[tokenIndex], out action.moveDirection) && action.moveDirection != TmlxDirection.nowhere)
        {
            tokenIndex++;
        }

        // Parse exit or goto
        if (tokenIndex < tokens.Count && tokens[tokenIndex] == "goto")
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
        if (number && INT_TO_COLOR_OR_CONDITION_DICTIONARY.ContainsKey(bit))
        {
            color = INT_TO_COLOR_OR_CONDITION_DICTIONARY[bit];
            error = null;
            return true;
        }

        if (!number && STRING_TO_COLOR_DICTIONARY.ContainsKey(tokens[tokenIndex]))
        {
            color = STRING_TO_COLOR_DICTIONARY[tokens[tokenIndex]];
            error = null;
            return true;
        }

        color = Color.clear;
        error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: '{tokens[tokenIndex]}' is not a valid color";
        return false;
    }

    private static bool ParseCondition(int lineIndex, List<string> tokens, ref int tokenIndex, out Color32 condition, out string error)
    {
        if (++tokenIndex >= tokens.Count)
        {
            condition = Color.clear;
            error = $"Line {lineIndex + 1}: condition is missing";
            return false;
        }

        bool number = int.TryParse(tokens[tokenIndex], out int bit);
        if (number && INT_TO_COLOR_OR_CONDITION_DICTIONARY.ContainsKey(bit))
        {
            condition = INT_TO_COLOR_OR_CONDITION_DICTIONARY[bit];
            error = null;
            return true;
        }

        if (!number && STRING_TO_CONDITION_DICTIONARY.ContainsKey(tokens[tokenIndex]))
        {
            condition = STRING_TO_CONDITION_DICTIONARY[tokens[tokenIndex]];
            error = null;
            return true;
        }

        condition = Color.clear;
        error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: '{tokens[tokenIndex]}' is not a valid condition";
        return false;
    }

    private static bool Precompute(TmlxInstruction[] instructions, HashSet<TmlxBranch> visitedBranches, ref TmlxAction action, Color32 color)
    {
        if (action.moveDirection != TmlxDirection.nowhere || action.gotoLineIndex < 0)
        {
            return true;
        }

        Color32 writeColor = action.writeColor;
        if (writeColor.a > 0)
        {
            color = writeColor;
        }

        TmlxBranch branch = new()
        {
            color = color,
            lineIndex = action.gotoLineIndex
        };
        if (!visitedBranches.Add(branch))
        {
            return false;
        }

        ref TmlxInstruction instruction = ref instructions[action.gotoLineIndex];
        ref TmlxAction newAction = ref (color.Equals(TmlxRuntime.COLOR_PIXEL_WHITE) ? ref instruction.whiteAction : ref instruction.colorAction);
        if (!Precompute(instructions, visitedBranches, ref newAction, color))
        {
            return false; // Keep looping branch
        }

        _ = visitedBranches.Remove(branch); // Remove non looping branch
        action = newAction;
        return true;
    }

    private static void Preoptimise(TmlxInstruction[] preoptimisedInstructions, HashSet<TmlxBranch> visitedBranches, ref TmlxAction action, Color32 color)
    {
        if (action.gotoLineIndex < 0)
        {
            return;
        }

        ref TmlxInstruction preoptimisedInstruction = ref preoptimisedInstructions[action.gotoLineIndex];
        if (preoptimisedInstruction.instructionType != TmlxInstructionType.nothing)
        {
            return;
        }

        Color32 writeColor = action.writeColor;
        if (writeColor.a > 0)
        {
            color = writeColor;
        }

        TmlxBranch branch = new()
        {
            color = color,
            lineIndex = action.gotoLineIndex
        };
        if (visitedBranches.Add(branch))
        {
            Preoptimise(preoptimisedInstructions, visitedBranches, ref preoptimisedInstruction.whiteAction, TmlxRuntime.COLOR_PIXEL_WHITE);
            Preoptimise(preoptimisedInstructions, visitedBranches, ref preoptimisedInstruction.colorAction, TmlxRuntime.COLOR_PIXEL_BLACK);
        }

        action.gotoLineIndex = preoptimisedInstruction.whiteAction.gotoLineIndex;
    }

    private static bool Process(string text, out TmlxInstruction[] instructions, out Dictionary<string, int> labelsDictionary, out int instructionCount, out string error)
    {
        List<TmlxInstruction> instructionList = new();
        labelsDictionary = new Dictionary<string, int>();
        instructionCount = 0;
        int blockCommentStartIndex = -1;
        string[] lines = text.Split('\n');
        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            TmlxInstruction instruction = new();
            List<string> tokens = Tokenize(lineIndex, lines[lineIndex], ref blockCommentStartIndex);
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

                if (labelsDictionary.ContainsKey(tokens[tokenIndex]))
                {
                    instructions = null;
                    error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: label '{tokens[tokenIndex]}' is already defined on line {labelsDictionary[tokens[tokenIndex]] + 1}";
                    return false;
                }

                labelsDictionary.Add(tokens[tokenIndex++], lineIndex);
                tokenIndex++;
            }

            int oldTokenIndex = tokenIndex;
            if (tokenIndex < tokens.Count && (tokens[tokenIndex] == "if" || tokens[tokenIndex] == "while"))
            {
                bool loop = tokens[tokenIndex] == "while";
                if (!ParseCondition(lineIndex, tokens, ref tokenIndex, out Color32 condition, out string colorError))
                {
                    instructions = null;
                    error = colorError;
                    return false;
                }

                if (++tokenIndex >= tokens.Count)
                {
                    instructions = null;
                    error = $"Line {lineIndex + 1}: '{(loop ? "while" : "if")}' action is missing";
                    return false;
                }

                oldTokenIndex = tokenIndex;
                if (!ParseAction(lines, lineIndex, tokens, ref tokenIndex, loop, out TmlxAction consequentAction, out string actionError))
                {
                    instructions = null;
                    error = actionError;
                    return false;
                }

                if (tokenIndex == oldTokenIndex)
                {
                    instructions = null;
                    error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: found '{tokens[tokenIndex]}' before '{(loop ? "while" : "if")}' action";
                    return false;
                }

                TmlxAction alternativeAction;
                if (tokenIndex >= tokens.Count || tokens[tokenIndex] != "else")
                {
                    alternativeAction = new TmlxAction
                    {
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
                    if (!ParseAction(lines, lineIndex, tokens, ref tokenIndex, false, out alternativeAction, out actionError))
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

                instruction.colorAction = condition.Equals(TmlxRuntime.COLOR_PIXEL_BLACK) ? consequentAction : alternativeAction;
                instruction.whiteAction = condition.Equals(TmlxRuntime.COLOR_PIXEL_WHITE) ? consequentAction : alternativeAction;
                instruction.instructionType = TmlxInstructionType.conditional;
            }
            else if (ParseAction(lines, lineIndex, tokens, ref tokenIndex, false, out TmlxAction action, out string actionError))
            {
                instruction.colorAction = instruction.whiteAction = action;
                if (tokenIndex != oldTokenIndex)
                {
                    instruction.instructionType = TmlxInstructionType.unconditional;
                }
            }
            else
            {
                instructions = null;
                error = actionError;
                return false;
            }

            // Check end
            if (tokenIndex < tokens.Count)
            {
                instructions = null;
                error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: '{tokens[tokenIndex]}' {(tokenIndex == oldTokenIndex ? "is not a valid instruction" : "should be on the next line")}";
                return false;
            }

            if (instruction.instructionType != TmlxInstructionType.nothing)
            {
                instructionCount++;
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

    private static bool Resolve(int processedInstructionIndex, Dictionary<string, int> labelsDictionary, ref TmlxAction action, out string error)
    {
        if (action.gotoLabel == null)
        {
            error = null;
            return true;
        }

        if (!labelsDictionary.ContainsKey(action.gotoLabel))
        {
            error = $"Line {processedInstructionIndex + 1}: label '{action.gotoLabel}' is not defined";
            return false;
        }

        action.gotoLineIndex = labelsDictionary[action.gotoLabel];
        action.gotoLabel = null;
        error = null;
        return true;
    }
}
