using UnityEngine;

public class LetterDropZone : MonoBehaviour
{
    [Tooltip("What letter belongs in this exact spot?")]
    public string correctLetterRequired;

    public WordJumbleManager wordManager;

    private bool isFilled = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isFilled) return; // Already has a letter in it!

        LetterIdentity letterTile = collision.GetComponent<LetterIdentity>();
        DragDropItem dragItem = collision.GetComponent<DragDropItem>();

        // If it's a letter, and the player just let go of it...
        if (letterTile != null && dragItem != null && dragItem.isReleased)
        {
            // Does the letter match this specific slot? (Case-insensitive check)
            if (letterTile.myLetter.ToUpper() == correctLetterRequired.ToUpper())
            {
                LockLetterInPlace(letterTile, dragItem);
            }
            else
            {
                // WRONG LETTER! Bounce it back or play an error sound
                Debug.Log($"<color=red>Wrong!</color> This slot needs {correctLetterRequired}, not {letterTile.myLetter}");
                if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
                
                // Optional: You can reset its position here!
            }
        }
    }

    private void LockLetterInPlace(LetterIdentity letterTile, DragDropItem dragItem)
    {
        isFilled = true;
        letterTile.isLockedIn = true;

        // Snap the letter perfectly to the center of this slot
        letterTile.transform.position = transform.position;

        // Disable dragging so they can't mess it up now that it's right
        Destroy(dragItem); 
        
        // Play success sound
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        Debug.Log($"<color=green>Nice!</color> {letterTile.myLetter} locked in!");

        // Tell the manager to check if the whole word is done!
        wordManager.CheckIfWordIsComplete();
    }
}