using UnityEngine;
using TMPro;
using System.Text;

public class SinhalaFixer : MonoBehaviour
{
    public TMP_Text textComponent;

    // Run this whenever the object starts to fix static text
    void OnEnable()
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = FixSinhala(textComponent.text);
        }
    }

    // Call this function from your Controller when setting new text!
    public void SetText(string text)
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
        textComponent.text = FixSinhala(text);
    }

    public static string FixSinhala(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        StringBuilder sb = new StringBuilder();
        char[] chars = text.ToCharArray();

        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            
            // CHECK 1: Is this a "Pre-Vowel"? (Kombuva ෙ, Dig Kombuva ේ, Kombu Deva ෛ)
            // Unicode: \u0DD9, \u0DDA, \u0DDB
            if (c == '\u0DD9' || c == '\u0DDA' || c == '\u0DDB')
            {
                // SWAP LOGIC:
                // If we have a previous character (the consonant), insert this vowel BEFORE it.
                if (sb.Length > 0)
                {
                    char consonant = sb[sb.Length - 1]; // Get the consonant (e.g. 'k')
                    sb.Remove(sb.Length - 1, 1);        // Remove 'k'
                    sb.Append(c);                       // Add 'e'
                    sb.Append(consonant);               // Add 'k' back -> Result: "ek" (Looks like කෙ)
                }
                else
                {
                    sb.Append(c);
                }
            }
            // CHECK 2: Handle the "O" sounds (Kombuva + Aela-pilla ො) which are tricky
            // If your font splits these, you might need to handle \u0DDC separately.
            // For now, standard Unicode usually treats ො as one block, but if it fails, let me know.
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }
}