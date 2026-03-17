using UnityEngine;

public class DaysSequenceController : BaseLevelController
{
    // Tell the base class what game mode this is
    protected override GameType SupportedGameType => GameType.DaysSequence;

    protected override void InitializeLevel()
    {
        // We have 7 days in a week, so the target score is always 7!
        targetScore = 7; 
    }

    // Call this when a day is placed in the CORRECT slot
    public void DayPlacedCorrectly()
    {
        HandleCorrectAnswer(); // Adds points
        CheckWinCondition();   // Checks if we hit 7 points to win the level!
    }

    // Call this when a day is placed in the WRONG slot
    public void DayPlacedWrong()
    {
        HandleWrongAnswer(); // Deducts points, adds to mistake count, shakes screen, etc.
        if (AudioManager.Instance != null) AudioManager.Instance.PlayError();
    }
}