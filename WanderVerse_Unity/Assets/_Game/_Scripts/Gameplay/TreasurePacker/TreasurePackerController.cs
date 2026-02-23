using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class TreasurePackerController : BaseLevelController
{
    [Header("--- PLACE VALUE UI ---")]
    public TMP_Text signPromptText;  // The instruction sign
    public TMP_Text counterText;     // Shows "0 / 45"

    [Header("--- SPAWNER SETTINGS ---")]
    public GameObject goldBarPrefab;
    public GameObject singleCoinPrefab;
    public Transform barSpawnPoint;  // Where bars shoot from (e.g., a pipe or off-screen)
    public Transform coinSpawnPoint; // Where coins shoot from
    public int batchSpawnAmount = 5; // How many to shoot out at once
    public float throwForceUp = 10f; // How high they shoot
    public float throwForceSide = 3f; // How wide they scatter
    [Tooltip("Time in seconds between each item shooting out")]
    public float spawnDelay = 0.5f;

    // Local Round State
    private int currentRoundTargetValue;
    private int currentChestValue = 0;
    private bool isRoundTransitioning = false; // Prevents dragging while waiting for next round

    // Tracking the physical items
    private int activeBarsInScene = 0;
    private int activeCoinsInScene = 0;

    // Safety flags to prevent multiple fountains at the same time
    private bool isSpawningBars = false;
    private bool isSpawningCoins = false;

    // Our "Deck" of random rounds
    private List<TreasureRound> availableRounds = new List<TreasureRound>();

    // 1. Tell the Base Controller what type of game this is
    protected override GameType SupportedGameType => GameType.PlaceValue;

    // 2. Initialize our specific Place Value data
    protected override void InitializeLevel()
    {
        // Copy the rounds from LevelData into our temporary deck so we can pull them randomly
        if (levelData.treasureRounds != null)
        {
            availableRounds = new List<TreasureRound>(levelData.treasureRounds);
        }
        else
        {
            Debug.LogError("[TreasurePacker] No Treasure Rounds found in LevelData!");
        }
    }

    // 3. Override StartGame to reset our specific UI after the base controller sets up
    protected override void StartGame()
    {
        base.StartGame(); // This resets currentScore, mistakeCount, and starts timer
       // Start the Coroutines to shoot the first batch!
        StartCoroutine(SpawnBarsRoutine());
        StartCoroutine(SpawnCoinsRoutine());
        
        LoadNextRound();
    }
// --- NEW: ASYNC SPAWNING COROUTINES ---

    private IEnumerator SpawnBarsRoutine()
    {
        isSpawningBars = true;
        
        for (int i = 0; i < batchSpawnAmount; i++)
        {
            GameObject newBar = Instantiate(goldBarPrefab, barSpawnPoint.position, Quaternion.identity);
            activeBarsInScene++;
            ThrowItem(newBar);
            
            // Wait for half a second before looping again
            yield return new WaitForSeconds(spawnDelay); 
        }
        
        isSpawningBars = false;
    }

    private IEnumerator SpawnCoinsRoutine()
    {
        isSpawningCoins = true;
        
        for (int i = 0; i < batchSpawnAmount; i++)
        {
            GameObject newCoin = Instantiate(singleCoinPrefab, coinSpawnPoint.position, Quaternion.identity);
            activeCoinsInScene++;
            ThrowItem(newCoin);
            
            // Wait for half a second before looping again
            yield return new WaitForSeconds(spawnDelay);
        }
        
        isSpawningCoins = false;
    }

    private void ThrowItem(GameObject item)
    {
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; 

            // Calculate a random arc upwards and slightly sideways
            float randomX = Random.Range(-throwForceSide, throwForceSide);
            float randomY = Random.Range(throwForceUp * 0.8f, throwForceUp * 1.2f); 

            // Apply the physical force
            rb.AddForce(new Vector2(randomX, randomY), ForceMode2D.Impulse);
            
            // Give it a random spin
            rb.AddTorque(Random.Range(-15f, 15f));
        }
    }

    // --- UPDATED: CONSUME ITEM ---

    public void ConsumeItem(int amount)
    {
        if (!isGameActive || isRoundTransitioning) return;

        // 1. Keep track of what we just ate
        if (amount == 10) 
        {
            activeBarsInScene--;
            // Only start shooting if we hit 0 AND we aren't already shooting
            if (activeBarsInScene <= 0 && !isSpawningBars) 
            {
                StartCoroutine(SpawnBarsRoutine());
            }
        }
        else if (amount == 1) 
        {
            activeCoinsInScene--;
            // Only start shooting if we hit 0 AND we aren't already shooting
            if (activeCoinsInScene <= 0 && !isSpawningCoins) 
            {
                StartCoroutine(SpawnCoinsRoutine());
            }
        }

        // 2. Do the math
        currentChestValue += amount;
        UpdateCounterUI();

        // 3. Check Win/Loss logic
        if (currentChestValue == currentRoundTargetValue)
        {
            isRoundTransitioning = true;
            HandleCorrectAnswer(); 
            CheckWinCondition();   
            
            if (isGameActive) Invoke(nameof(LoadNextRound), 1.5f); 
        }
        else if (currentChestValue > currentRoundTargetValue)
        {
            Debug.Log("Too heavy! Try again.");
            HandleWrongAnswer(); 
            currentChestValue = 0; 
            UpdateCounterUI();
        }
    }

    private void LoadNextRound()
    {
        isRoundTransitioning = false;
        currentChestValue = 0;

        // Safety check: If we run out of questions before the level ends, refill the deck!
        if (availableRounds.Count == 0)
        {
            availableRounds = new List<TreasureRound>(levelData.treasureRounds);
        }

        // Pick a random round from the deck
        int randomIndex = Random.Range(0, availableRounds.Count);
        TreasureRound round = availableRounds[randomIndex];
        
        // Remove it so it doesn't repeat
        availableRounds.RemoveAt(randomIndex);

        // Setup the local round data
        currentRoundTargetValue = round.targetValue;
        
        if (signPromptText != null) 
            signPromptText.text = round.signPromptText;
        
        UpdateCounterUI();
    }

    // 4. Called by your ChestDropZone script when a coin/bar is dropped
    // public void AddGold(int amount)
    // {
    //     // Don't accept gold if the game is over or we are waiting for the next round
    //     if (!isGameActive || isRoundTransitioning) return;

    //     currentChestValue += amount;
    //     UpdateCounterUI();

    //     // Check the chest weight
    //     if (currentChestValue == currentRoundTargetValue)
    //     {
    //         // PERFECT MATCH!
    //         isRoundTransitioning = true;
            
    //         // Call the base controller to add points (pointsForCorrect)
    //         HandleCorrectAnswer(); 
            
    //         // Call the base controller to check if currentScore >= targetScore
    //         CheckWinCondition();   
            
    //         // If CheckWinCondition didn't end the level, move to the next round
    //         if (isGameActive) 
    //         {
    //             Debug.Log("Packing successful! Loading next chest...");
    //             Invoke(nameof(LoadNextRound), 1.5f); // Wait 1.5 seconds so player can see it full
    //         }
    //     }
    //     else if (currentChestValue > currentRoundTargetValue)
    //     {
    //         // TOO HEAVY!
    //         Debug.Log("Too much gold! Chest reset.");
            
    //         // Call base controller to log a mistake (and deduct points if applicable)
    //         HandleWrongAnswer(); 
            
    //         // Empty the chest so they can try again
    //         currentChestValue = 0; 
    //         UpdateCounterUI();
    //     }
    // }

    private void UpdateCounterUI()
    {
        if (counterText != null)
        {
            counterText.text = $"{currentChestValue} / {currentRoundTargetValue}";
        }
    }
}