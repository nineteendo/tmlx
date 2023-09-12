using UnityEngine;

namespace TMPro
{
    [AddComponentMenu("UI/TextMeshPro - Better Input Field", 11)]
    public class TMP_BetterInputField : TMP_InputField
    {
        protected override void Append(string input)
        {
            if (readOnly || !InPlaceEditing())
                return;

            
            foreach (char c in input)
                if (IsValidChar(c))  // More concise
                    Append(c);
        }

        protected override bool IsValidChar(char c)
        {
            // TODO - Fix bugs with text starting with \u0003
            return c >= '\t' && c <= '\v' || c == '\r' || c >= ' ' && c <= '~' || c >= '\u00A0';  // Allows only printable characters
        }

        private bool InPlaceEditing() // Needed for Append implementation
        {
            // RuntimePlatform.MetroPlayerX86, RuntimePlatform.MetroPlayerX64 & RuntimePlatform.MetroPlayerARM are obsolete
            if (Application.platform == RuntimePlatform.WSAPlayerX86 || Application.platform == RuntimePlatform.WSAPlayerX64 || Application.platform == RuntimePlatform.WSAPlayerARM)
                return !TouchScreenKeyboard.isSupported || TouchScreenKeyboard.isInPlaceEditingAllowed;

            if (TouchScreenKeyboard.isSupported && shouldHideSoftKeyboard)
                return true;

            if (TouchScreenKeyboard.isSupported && !shouldHideSoftKeyboard && !shouldHideMobileInput)
                return false;

            return true;
        }
    }
}
