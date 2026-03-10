using UnityEngine;

public class WordJumbleManager : MonoBehaviour
{
    [Header("--- Word Setup ---")]
    public string targetWord = "MONDAY";
    public int totalLettersRequired; // E.g., 6 for MONDAY
    private int lettersLocked = 0;

    [Header("--- Transition Setup ---")]
    public GameObject individualLettersContainer; // The parent holding the single letters
    public GameObject completedDayTile; // The full "MONDAY" tile that they will use in Phase 2

    private void Start()
    {
        // Hide the completed tile at the start
        if (completedDayTile != null) completedDayTile.SetActive(false);
    }

    public void CheckIfWordIsComplete()
    {
        lettersLocked++;

        if (lettersLocked >= totalLettersRequired)
        {
            WordCompleted();
        }
    }

    private void WordCompleted()
    {
        Debug.Log($"<color=cyan>WORD COMPLETE!</color> You spelled {targetWord}!");
        
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySuccess();

        // 1. Hide the individual letter slots
        individualLettersContainer.SetActive(false);

        // 2. Show the fully combined, draggable Day Tile for Phase 2!
        completedDayTile.SetActive(true);

        // TODO: You can now trigger a particle effect or tell the Main Controller to unlock the next part!
    }
}