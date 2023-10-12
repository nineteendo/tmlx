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
    nowhere,
    up,
    down,
    left,
    right
}

public enum BtmlInstructionType
{
    nothing,
    unconditional,
    conditional
}

public struct BtmlInstruction
{
    public BtmlAction blackAction;
    public BtmlAction whiteAction;
    public BtmlInstructionType instructionType;
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
    private static readonly string[] reservedWords = { ":", "black", "down", "else", "exit", "goto", "if", "left", "nowhere", "right", "up", "while", "white", "write" };

    public static bool Compile(string text, bool optimised, out BtmlInstruction[] instructions, out int instructionCount, out string error)
    {
        if (text == null)
        {
            error = "text is null";
            instructions = null;
            instructionCount = 0;
            return false;
        }

        // Process
        if (!Process(text, out BtmlInstruction[] processedInstructions, out Dictionary<string, int> labels, out instructionCount, out string processError))
        {
            error = processError;
            instructions = null;
            return false;
        }

        // Expand
        BtmlInstruction[] expandedInstructions = (BtmlInstruction[])processedInstructions.Clone();
        for (int expandedInstructionIndex = 0; expandedInstructionIndex < expandedInstructions.Length; expandedInstructionIndex++)
        {
            ref BtmlInstruction expandedInstruction = ref expandedInstructions[expandedInstructionIndex];
            ref BtmlAction whiteAction = ref expandedInstruction.whiteAction;
            ref BtmlAction blackAction = ref expandedInstruction.blackAction;
            if (whiteAction.writeColor.a <= 0x00)
            {
                whiteAction.writeColor = BtmlRuntime.COLOR_PIXEL_OFF;
            }

            if (blackAction.writeColor.a <= 0x00)
            {
                blackAction.writeColor = BtmlRuntime.COLOR_PIXEL_ON;
            }

            if (!Resolve(expandedInstructionIndex, labels, ref whiteAction, out string resolveError) || !Resolve(expandedInstructionIndex, labels, ref blackAction, out resolveError))
            {
                error = resolveError;
                instructions = null;
                return false;
            }
        }

        // Pre-optimise
        HashSet<BtmlBranch> visitedBranches = new(new BtmlBranchEqualityComparer());
        BtmlInstruction[] preoptimisedInstructions = (BtmlInstruction[])expandedInstructions.Clone();
        for (int preoptimisedInstructionIndex = 0; preoptimisedInstructionIndex < preoptimisedInstructions.Length; preoptimisedInstructionIndex++)
        {
            ref BtmlInstruction preoptimisedInstruction = ref preoptimisedInstructions[preoptimisedInstructionIndex];
            Preoptimise(preoptimisedInstructions, visitedBranches, ref preoptimisedInstruction.whiteAction);
            Preoptimise(preoptimisedInstructions, visitedBranches, ref preoptimisedInstruction.blackAction);
        }

        instructions = (BtmlInstruction[])preoptimisedInstructions.Clone();
        if (!optimised)
        {
            error = null;
            return true;
        }

        // Optimise
        visitedBranches.Clear();
        for (int instructionIndex = 0; instructionIndex < instructions.Length; instructionIndex++)
        {
            ref BtmlInstruction optimisedInstruction = ref instructions[instructionIndex];
            if (optimisedInstruction.instructionType != BtmlInstructionType.nothing)
            {
                _ = Precompute(instructions, visitedBranches, ref optimisedInstruction.whiteAction);
                _ = Precompute(instructions, visitedBranches, ref optimisedInstruction.blackAction);
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


    private static bool ParseAction(string[] lines, int lineIndex, List<string> tokens, ref int tokenIndex, bool loop, out BtmlAction action, out string error)
    {
        action = new BtmlAction
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
        if (tokenIndex < tokens.Count && Enum.TryParse(tokens[tokenIndex], out action.moveDirection) && action.moveDirection != BtmlDirection.nowhere)
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

    private static bool Precompute(BtmlInstruction[] instructions, HashSet<BtmlBranch> visitedBranches, ref BtmlAction action)
    {
        if (action.moveDirection != BtmlDirection.nowhere || action.gotoLineIndex < 0)
        {
            return true;
        }

        BtmlBranch branch = new()
        {
            color = action.writeColor,
            lineIndex = action.gotoLineIndex
        };
        if (!visitedBranches.Add(branch))
        {
            return false;
        }

        ref BtmlInstruction instruction = ref instructions[action.gotoLineIndex];
        ref BtmlAction newAction = ref (action.writeColor.Equals(BtmlRuntime.COLOR_PIXEL_OFF) ? ref instruction.whiteAction : ref instruction.blackAction);
        if (!Precompute(instructions, visitedBranches, ref newAction))
        {
            return false; // Keep looping branch
        }

        _ = visitedBranches.Remove(branch); // Remove non looping branch
        action = newAction;
        return true;
    }

    private static void Preoptimise(BtmlInstruction[] preoptimisedInstructions, HashSet<BtmlBranch> visitedBranches, ref BtmlAction action)
    {
        if (action.gotoLineIndex < 0)
        {
            return;
        }

        ref BtmlInstruction preoptimisedInstruction = ref preoptimisedInstructions[action.gotoLineIndex];
        if (preoptimisedInstruction.instructionType != BtmlInstructionType.nothing)
        {
            return;
        }

        BtmlBranch branch = new()
        {
            color = action.writeColor,
            lineIndex = action.gotoLineIndex
        };
        if (visitedBranches.Add(branch))
        {
            Preoptimise(preoptimisedInstructions, visitedBranches, ref preoptimisedInstruction.whiteAction);
            Preoptimise(preoptimisedInstructions, visitedBranches, ref preoptimisedInstruction.blackAction);
        }

        action.gotoLineIndex = preoptimisedInstruction.whiteAction.gotoLineIndex;
    }

    private static bool Process(string text, out BtmlInstruction[] instructions, out Dictionary<string, int> labels, out int instructionCount, out string error)
    {
        List<BtmlInstruction> instructionList = new();
        labels = new Dictionary<string, int>();
        instructionCount = 0;
        int blockCommentStartIndex = -1;
        string[] lines = text.Split('\n');
        for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
        {
            BtmlInstruction instruction = new();
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

                if (labels.ContainsKey(tokens[tokenIndex]))
                {
                    instructions = null;
                    error = $"Line {lineIndex + 1}, word {tokenIndex + 1}: label '{tokens[tokenIndex]}' is already defined on line {labels[tokens[tokenIndex]] + 1}";
                    return false;
                }

                labels.Add(tokens[tokenIndex++], lineIndex);
                tokenIndex++;
            }

            int oldTokenIndex = tokenIndex;
            if (tokenIndex < tokens.Count && (tokens[tokenIndex] == "if" || tokens[tokenIndex] == "while"))
            {
                bool loop = tokens[tokenIndex] == "while";
                if (!ParseColor(lineIndex, tokens, ref tokenIndex, out Color32 condition, out string colorError))
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
                if (!ParseAction(lines, lineIndex, tokens, ref tokenIndex, loop, out BtmlAction consequentAction, out string actionError))
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

                BtmlAction alternativeAction;
                if (tokenIndex >= tokens.Count || tokens[tokenIndex] != "else")
                {
                    alternativeAction = new BtmlAction
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

                instruction.blackAction = condition.Equals(BtmlRuntime.COLOR_PIXEL_ON) ? consequentAction : alternativeAction;
                instruction.whiteAction = condition.Equals(BtmlRuntime.COLOR_PIXEL_OFF) ? consequentAction : alternativeAction;
                instruction.instructionType = BtmlInstructionType.conditional;
            }
            else if (ParseAction(lines, lineIndex, tokens, ref tokenIndex, false, out BtmlAction action, out string actionError))
            {
                instruction.blackAction = instruction.whiteAction = action;
                if (tokenIndex != oldTokenIndex)
                {
                    instruction.instructionType = BtmlInstructionType.unconditional;
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

            if (instruction.instructionType != BtmlInstructionType.nothing)
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
