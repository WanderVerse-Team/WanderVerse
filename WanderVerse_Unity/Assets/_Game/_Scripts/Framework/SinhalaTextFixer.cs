using UnityEngine;
using TMPro;

public class SinhalaTextFixer : MonoBehaviour
{
    private TMP_Text textComponent;

    void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }

    // Call this whenever you change the text to fix the Sinhala positioning
    public void SetFixedText(string input)
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
        textComponent.text = FixSinhala(input);
    }

    private string FixSinhala(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // This is a simplified "Shaping" logic for Sinhala Unicode reordering
        // It handles the 'Kombuva' (ෙ) and other pre-vowels
        char[] chars = text.ToCharArray();
        string fixedText = "";

        for (int i = 0; i < chars.Length; i++)
        {
            // Check for Kombuva (ෙ), Kombuva + Aela-pilla (ො), etc.
            // Unicode for Kombuva is \u0DD9
            if (chars[i] == '\u0DD9' || chars[i] == '\u0DDA' || chars[i] == '\u0DDB')
            {
                // If there's a character before it, swap them visually
                if (fixedText.Length > 0)
                {
                    char lastChar = fixedText[fixedText.Length - 1];
                    fixedText = fixedText.Remove(fixedText.Length - 1);
                    fixedText += chars[i].ToString() + lastChar.ToString();
                }
                else
                {
                    fixedText += chars[i];
                }
            }
            else
            {
                fixedText += chars[i];
            }
        }
        return fixedText;
    }
}