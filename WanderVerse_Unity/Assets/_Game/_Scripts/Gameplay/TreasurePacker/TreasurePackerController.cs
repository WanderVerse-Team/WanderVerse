using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class TreasurePackerController : BaseLevelController
{
    [Header("--- AUDIO SETTINGS ---")]
    public AudioClip barSpawnSound;
    public AudioClip coinSpawnSound;

    [Header("--- CHEST VISUALS ---")]
    [Tooltip("Drag the GameObject that has the SpriteRenderer for your chest here")]
    public SpriteRenderer chestVisual; 
    public Sprite chestOpenSprite;
    public Sprite chestClosedSprite;

    [Header("--- PLACE VALUE UI ---")]
    public TMP_Text signPromptText;  
    public TMP_Text tensCounter; 
    public TMP_Text onesCounter;     

    [Header("--- SPAWNER SETTINGS ---")]
    public GameObject goldBarPrefab;
    public GameObject singleCoinPrefab;
    public Transform barSpawnPoint;  
    public Transform coinSpawnPoint; 
    public int batchSpawnAmount = 5; // How many to shoot out at once

    [Header("--- THROW PHYSICS ---")]
    [Tooltip("Total force applied to the item")]
    public float throwForce = 12f; 
    
    [Tooltip("Angle in degrees. 0=Right, 90=Up, 180=Left")]
    public float barThrowAngle = 60f;   // Shoots up and to the right
    
    [Tooltip("Angle in degrees. 0=Right, 90=Up, 180=Left")]
    public float coinThrowAngle = 120f; // Shoots up and to the left
    
    [Tooltip("How much randomness to add to the angle so they don't all follow the exact same line")]
    public float throwSpread = 15f;
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

    
    private List<TreasureRound> availableRounds = new List<TreasureRound>();

    // Tell the Base Controller what type of game this is
    protected override GameType SupportedGameType => GameType.PlaceValue;

    
    protected override void InitializeLevel()
    {
        // Copy the rounds from LevelData into a temporary deck so to pull them randomly
        if (levelData.treasureRounds != null)
        {
            availableRounds = new List<TreasureRound>(levelData.treasureRounds);
        }
        else
        {
            Debug.LogError("[TreasurePacker] No Treasure Rounds found in LevelData!");
        }
    }


    
    protected override void BeginLevel()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        base.BeginLevel(); 
        
        // This shoots the coins for the Treasure Packer
        StartCoroutine(SpawnBarsRoutine());
        StartCoroutine(SpawnCoinsRoutine());
        LoadNextRound();
    }


    private IEnumerator SpawnBarsRoutine()
    {
        isSpawningBars = true;
        
        for (int i = 0; i < batchSpawnAmount; i++)
        {
            GameObject newBar = Instantiate(goldBarPrefab, barSpawnPoint.position, Quaternion.identity);

            // Play the spawn sound for each bar
            if (AudioManager.Instance != null && barSpawnSound != null)
            {
                AudioManager.Instance.PlaySFX(barSpawnSound);
            }
            activeBarsInScene++;
            ThrowItem(newBar, barThrowAngle);
            
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

            // Play the spawn sound for each coin
            if (AudioManager.Instance != null && coinSpawnSound != null)
            {
                AudioManager.Instance.PlaySFX(coinSpawnSound);
            }
            activeCoinsInScene++;
            ThrowItem(newCoin, coinThrowAngle);
            
            // Wait for half a second before looping again
            yield return new WaitForSeconds(spawnDelay);
        }
        
        isSpawningCoins = false;
    }

    private void ThrowItem(GameObject item, float baseAngle)
    {
        Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic; 

            // Add some random spread 
            float finalAngle = baseAngle + Random.Range(-throwSpread, throwSpread);
            
            
            float angleRad = finalAngle * Mathf.Deg2Rad;

            // Calculate the exact X and Y direction using Cos and Sin
            Vector2 throwDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

            // Apply the force 
            float randomForce = Random.Range(throwForce * 0.8f, throwForce * 1.2f);
            rb.AddForce(throwDirection * randomForce, ForceMode2D.Impulse);
            
            
            rb.AddTorque(Random.Range(-15f, 15f));
        }
    }

   

    public void ConsumeItem(int amount)
    {
        if (!isGameActive || isRoundTransitioning) return;

        // Keep track of the number of item in the scene for spawning.
        if (amount == 10) 
        {
            activeBarsInScene--;
            if (activeBarsInScene <= 0 && !isSpawningBars) StartCoroutine(SpawnBarsRoutine());
        }
        else if (amount == 1) 
        {
            activeCoinsInScene--;
            if (activeCoinsInScene <= 1 && !isSpawningCoins) StartCoroutine(SpawnCoinsRoutine());
        }

        
        currentChestValue += amount;
        UpdateCounterUI();
    }
    

    private void LoadNextRound()
    {
        isRoundTransitioning = false;
        currentChestValue = 0;

        OpenChest(); // Make sure the chest is open for the next round

        
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
    
    public void ConfirmChest()
    {
        if (!isGameActive || isRoundTransitioning) return;

        if (chestVisual != null && chestClosedSprite != null)
        {
            chestVisual.sprite = chestClosedSprite;
        }

        // Check if it packed the exact right amount
        if (currentChestValue == currentRoundTargetValue)
        {
            Debug.Log("Perfect Match! You locked in the right amount.");
            isRoundTransitioning = true;
            HandleCorrectAnswer(); 
            CheckWinCondition();   
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySuccess();
            }
            
            if (isGameActive) Invoke(nameof(LoadNextRound), 1.5f); 
        }
        else 
        {
            // If the answer is wrong.
            Debug.Log($"Wrong amount! You packed {currentChestValue}, but needed {currentRoundTargetValue}.");
            HandleWrongAnswer(); 
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayError();
            }
            
            // Empty the chest so to try again
            currentChestValue = 0; 
            UpdateCounterUI();

            //Open the chest again
            Invoke(nameof(OpenChest), 0.5f);
        }
    }

    private void OpenChest()
    {
        if (chestVisual != null && chestOpenSprite != null)
        {
            chestVisual.sprite = chestOpenSprite;
        }
    }

    private void UpdateCounterUI()
    {
        if (tensCounter != null)
        {
            tensCounter.text = $"{currentChestValue / 10}";
        }
        if (onesCounter != null)
        {
            onesCounter.text = $"{currentChestValue % 10}";
        }
    }
}