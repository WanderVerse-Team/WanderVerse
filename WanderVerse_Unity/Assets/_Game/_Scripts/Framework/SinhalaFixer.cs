using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SinhalaFixer : MonoBehaviour
{
    public TMP_Text textComponent;

    void OnEnable()
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
        if (textComponent != null)
        {
            textComponent.text = FixSinhala(textComponent.text);
        }
    }

    public void SetText(string text)
    {
        if (textComponent == null) textComponent = GetComponent<TMP_Text>();
        textComponent.text = FixSinhala(text);
    }

    public static string FixSinhala(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        List<char> result = new List<char>();

        foreach (char c in text)
        {
            // 1. Left-Side Vowels: Kombuva (ෙ), Diga Kombuva (ේ), Kombu Deweka (ෛ)
            if (c == '\u0DD9' || c == '\u0DDA' || c == '\u0DDB')
            {
                if (result.Count > 0)
                {
                    char prev = result[result.Count - 1]; // Grab the consonant (e.g., 'ද')
                    result.RemoveAt(result.Count - 1);    
                    result.Add(c);                        // Put Vowel first (e.g., 'ෙ')
                    result.Add(prev);                     // Put Consonant next (e.g., 'ද') -> ෙද
                }
                else result.Add(c);
            }
            
            // 2. Split Vowel: Kombuva haa Aela-pilla (ො)
            // Used in: එකොළහ, දොළහ, පහළොව
            else if (c == '\u0DDC')
            {
                if (result.Count > 0)
                {
                    char prev = result[result.Count - 1]; // Grab the consonant (e.g., 'ළ')
                    result.RemoveAt(result.Count - 1);
                    result.Add('\u0DD9');                 // Put Kombuva BEFORE ('ෙ')
                    result.Add(prev);                     // Put Consonant ('ළ')
                    result.Add('\u0DCF');                 // Put Aela-pilla AFTER ('ා') -> ෙළා
                }
                else result.Add(c);
            }
            
            // 3. Split Vowel: Kombuva haa Diga Aela-pilla (ෝ)
            else if (c == '\u0DDD')
            {
                if (result.Count > 0)
                {
                    char prev = result[result.Count - 1]; 
                    result.RemoveAt(result.Count - 1);
                    result.Add('\u0DD9');                 // Kombuva
                    result.Add(prev);                     // Consonant
                    result.Add('\u0DCF');                 // Aela-pilla
                    result.Add('\u0DCA');                 // Hal kirima (to make it Diga)
                }
                else result.Add(c);
            }
            
            // 4. Split Vowel: Kombuva haa Gayanukitta (ෞ)
            else if (c == '\u0DDE')
            {
                if (result.Count > 0)
                {
                    char prev = result[result.Count - 1];
                    result.RemoveAt(result.Count - 1);
                    result.Add('\u0DD9');                 // Kombuva
                    result.Add(prev);                     // Consonant
                    result.Add('\u0DDF');                 // Gayanukitta
                }
                else result.Add(c);
            }
            
            // 5. Normal characters (Consonants, top/bottom vowels)
            else
            {
                result.Add(c);
            }
        }

        return new string(result.ToArray());
    }
}