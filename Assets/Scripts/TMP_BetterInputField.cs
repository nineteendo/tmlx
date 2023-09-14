using TMPro;
using UnityEngine;

[AddComponentMenu("UI/TextMeshPro - Better Input Field", 11)]
public class TMP_BetterInputField : TMP_InputField
{
    protected override void Append(string input)
    {
        if (readOnly || !InPlaceEditing())
        {
            return;
        }

        foreach (char c in input)
        {
            if (IsValidChar(c))  // More concise
            {
                Append(c);
            }
        }
    }

    protected override bool IsValidChar(char c)
    {
        // TODO - Fix bugs with text starting with \u0003
        return (multiLine && c is (>= '\t' and <= '\v') or '\r') || c is (>= ' ' and <= '~') or >= '\u00A0';  // Allows only printable characters
    }


    private bool InPlaceEditing() // Needed for Append implementation
    {
        // RuntimePlatform.MetroPlayerX86, RuntimePlatform.MetroPlayerX64 & RuntimePlatform.MetroPlayerARM are obsolete
        return Application.platform is RuntimePlatform.WSAPlayerX86 or RuntimePlatform.WSAPlayerX64 or RuntimePlatform.WSAPlayerARM
            ? !TouchScreenKeyboard.isSupported || TouchScreenKeyboard.isInPlaceEditingAllowed
            : (TouchScreenKeyboard.isSupported && shouldHideSoftKeyboard)
|| !TouchScreenKeyboard.isSupported || shouldHideSoftKeyboard || shouldHideMobileInput;
    }
}
