using UnityEngine;
using TMPro;

public class TreasurePackerController : MonoBehaviour
{
    [Header("--- Level Data ---")]
    public LevelData levelData;
    private int currentRoundIndex = 0;

    [Header("--- UI Elements ---")]
    public TMP_Text signPromptText;  // The instruction sign
    public TMP_Text counterText;     // Shows "0 / 45"

    private int targetValue;
    private int currentValue = 0;

    void Start()
    {
        LoadRound();
    }

    void LoadRound()
    {
        currentValue = 0;
        
        // Grab data from your master LevelData file
        TreasureRound currentRound = levelData.treasureRounds[currentRoundIndex];
        targetValue = currentRound.targetValue;
        
        // Update the legacy Sinhala text
        signPromptText.text = currentRound.signPromptText;
        
        UpdateCounterUI();
    }

    public void AddGold(int amount)
    {
        // Add the coin/bar value to our total
        currentValue += amount;
        UpdateCounterUI();

        // Check Win/Loss conditions
        if (currentValue == targetValue)
        {
            Debug.Log("Correct! Moving to next round...");
            Invoke("NextRound", 2f); // Wait 2 seconds, then go to next round
        }
        else if (currentValue > targetValue)
        {
            Debug.Log("Oh no, too much gold! Try again.");
            currentValue = 0; // Reset the chest
            UpdateCounterUI();
        }
    }

    void UpdateCounterUI()
    {
        // Updates the text on screen to look like "20 / 45"
        counterText.text = currentValue + " / " + targetValue;
    }

    void NextRound()
    {
        currentRoundIndex++;
        if (currentRoundIndex < levelData.treasureRounds.Count)
        {
            LoadRound();
        }
        else
        {
            Debug.Log("Level Complete! Give Player XP.");
        }
    }
}