using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using WanderVerse.Backend.Services;
using WanderVerse.Framework.Data;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // Parameters: LevelID, Score, XP Added, Stars, Is New Highscore
    public event Action<string, int, int, int, bool> OnLevelCompleted;

    private LevelData pendingLevelData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this from level menu buttons
    public void LoadLevel(LevelData dataToLoad, string sceneName) 
    {
        pendingLevelData = dataToLoad;

        SceneManager.LoadScene(sceneName);
    }

    // Called by BaseLevelController on Start()
    public LevelData GetPendingLevelData()
    {
        LevelData dataToReturn = pendingLevelData;

        pendingLevelData = null;

        return dataToReturn;
    }

    public void ProcessLevelCompletion(string levelID, int mistakes, LevelData levelData)
    {
        // Prevent empty IDs from corrupting the save file
        if (string.IsNullOrEmpty(levelID) || levelID == CourseCatalog.NONE) 
        {
            Debug.LogError($"[GameManager] CRITICAL ERROR: Invalid LevelID '{levelID}'! Progress will not be saved. Please assign a valid LevelID in your LevelData scriptable object.");
            return;
        }

        // CALCULATE XP

        // Formula: MaxPossibleXP - (Mistakes * Penalty)
        int rawScore = levelData.maxXpReward - (mistakes * levelData.xpDeductionPerMistake);

        // Clamp: Ensure it never drops below Base, and never goes above Max
        int currentRunScore = Mathf.Clamp(rawScore, levelData.baseXpReward, levelData.maxXpReward);

        // CALCULATE STARS

        int stars = CalculateStars(mistakes, levelData);

        Debug.Log($"[GameManager] Level {levelID} Finished.");
        Debug.Log($"Mistakes: {mistakes} | Score: {currentRunScore} | Stars: {stars}");

        // HIGH SCORE LOGIC

        int previousBest = CloudSyncManager.Instance.GetHighscoreForLevel(levelID);
        int xpToAdd = 0;
        bool isNewBest = false;

        if (currentRunScore > previousBest)
        {
            xpToAdd = currentRunScore - previousBest;
            CloudSyncManager.Instance.SetHighscoreForLevel(levelID, currentRunScore, stars);
            isNewBest = true;
            Debug.Log($"New Personal Best! +{xpToAdd} XP added to Total XP");
        }
        else
        {
            // Logic to handle a perfect revision (0 mistakes)

            if (mistakes == 0)
            {
                xpToAdd = levelData.perfectRunBonus;
                Debug.Log($"Perfect Revision Bonus: +{xpToAdd} XP");
            }
        }

        // SAVE

        if (xpToAdd > 0)
        {
            CloudSyncManager.Instance.AddTotalXP(xpToAdd);
        }

        // BROADCAST THE EVENT
        OnLevelCompleted?.Invoke(levelID, currentRunScore, xpToAdd, stars, isNewBest);

    }

    private int CalculateStars(int mistakes, LevelData levelData)
    {
        if (mistakes <= levelData.maxMistakesFor3Stars) return 3;
        if (mistakes <= levelData.maxMistakesFor2Stars) return 2;
        return 1;
    }
}
