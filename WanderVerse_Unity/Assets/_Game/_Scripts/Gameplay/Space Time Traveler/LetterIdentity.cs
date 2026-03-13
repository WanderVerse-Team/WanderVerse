using UnityEngine;

public class LetterIdentity : MonoBehaviour
{
    [Tooltip("Type the single letter here, e.g., 'M', 'O', 'N'")]
    public string myLetter;

    [HideInInspector]
    public bool isLockedIn = false; // Prevents it from being moved once correct
}