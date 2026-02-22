using UnityEngine;
using TMPro;
using System.Collections;

public class TreasurePackerController : BaseLevelController
{
    [Header("--- UI References ---")]
    public TextMeshProUGUI signText; // The sign saying "Pack 45"
    public SinhalaFixer sinhalaFixer;

    [Header("--- Game State ---")]
    public int targetNumber;
    private int currentTotal = 0;

    [Header("--- Prefabs/Objects ---")]
    public GameObject chestOpenEffect; // Visual sparkle when full

    protected override GameType SupportedGameType => GameType.PlaceValue;

    protected override void InitializeLevel()
    {
        // For testing, let's pick 45. 
        // In the real game, this would come from your LevelData questions.
        targetNumber = 45; 
        
        // Convert number to Sinhala Legacy text: "45ක් ඇසුරුම් කරන්න"
        // (You would get the converted string from your data file)
        if(sinhalaFixer != null)
            sinhalaFixer.SetText("45la weiqreï lrkak"); 

        currentTotal = 0;
    }

    public void AddValue(int value)
    {
        currentTotal += value;
        Debug.Log("Current Chest Value: " + currentTotal);

        if (currentTotal == targetNumber)
        {
            WinGame();
        }
        else if (currentTotal > targetNumber)
        {
            ResetChest(); // Too much gold!
        }
    }

    private void WinGame()
    {
        Debug.Log("Treasure Packed Perfectly!");
        HandleCorrectAnswer();
        // Trigger animations/audio
    }

    private void ResetChest()
    {
        Debug.Log("Too heavy! Try again.");
        currentTotal = 0;
        // Logic to return all bars/coins to their starting positions
    }
}