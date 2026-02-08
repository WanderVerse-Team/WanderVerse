using UnityEngine;
using WanderVerse.Backend.Services;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

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

    public void ProcessLevelCompletion(string levelID, int mistakes, LevelData levelData)
    {
        // 1. CALCULATE XP

        // Formula: MaxPossibleXP - (Mistakes * Penalty)
        int rawScore = levelData.maxXpReward - (mistakes * levelData.xpDeductionPerMistake);

        // Clamp: Ensure it never drops below Base, and never goes above Max
        int currentRunScore = Mathf.Clamp(rawScore, levelData.baseXpReward, levelData.maxXpReward);

        // 2. CALCULATE STARS

        int stars = CalculateStars(mistakes, levelData);

        Debug.Log($"[GameManager] Level {levelID} Finished.");
        Debug.Log($"Mistakes: {mistakes} | Score: {currentRunScore} | Stars: {stars}");

        // 3. HIGH SCORE LOGIC

        int previousBest = CloudSyncManager.Instance.GetHighscoreForLevel(levelID);
        int xpToAdd = 0;

        if (currentRunScore > previousBest)
        {
            xpToAdd = currentRunScore - previousBest;
            CloudSyncManager.Instance.SetHighscoreForLevel(levelID, currentRunScore, stars);
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

        // 4. SAVE

        if (xpToAdd > 0)
        {
            CloudSyncManager.Instance.AddTotalXP(xpToAdd);
        }

    }

    private int CalculateStars(int mistakes, LevelData levelData)
    {
        if (mistakes <= levelData.maxMistakesFor3Stars) return 3;
        if (mistakes <= levelData.maxMistakesFor2Stars) return 2;
        return 1;
    }
}
