using UnityEngine;

public abstract class BaseLevelController : MonoBehaviour
{
    [Header("--- LEVEL DATA ---")]
    [Tooltip("Drag the LevelData ScriptableObject here")]
    [SerializeField] protected LevelData levelData;

    // Override this in the child class and add the GameType it supports
    protected abstract GameType SupportedGameType { get; }

    protected int targetScore;
    protected int pointsForCorrect;
    public int pointsForWrong;
    protected bool useTimer;
    protected float levelTimeLimit;
    public int maxMistakes;

    // STATE
    protected int currentScore = 0;
    protected float timeRemaining;
    protected int mistakeCount = 0;
    protected bool isGameActive = false;

    // Events to update the UI
    public System.Action<int> OnScoreUpdated;
    public System.Action<float> OnTimerUpdated;
    public System.Action<bool> OnLevelEnded;

    protected virtual void Awake()
    {
        // If you need to initialize any level specific internal variables, do it here
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        if (!ValidateLevelData()) return;

        LoadBaseData();
        InitializeLevel();

        // Play background music
        if (AudioManager.Instance != null && levelData.backgroundMusic != null) 
        {
            AudioManager.Instance.PlayMusic(levelData.backgroundMusic );
        }

        if (GameManager.Instance != null)
        {
            Debug.Log($"[BaseLevelController] Waiting for GameManager to start the {gameObject.name}...");
        }
        else
        {
            Debug.LogWarning("[BaseLevelController] No GameManager found! Starting in DEBUG mode.");

            //StartGame();
        }

        StartGame();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (isGameActive && useTimer)
        {
            Timer();
        }
        else return;
    }

    private bool ValidateLevelData()
    {
        if (levelData == null)
        {
            Debug.LogError($"[CRITICAL] No LevelData assigned to {gameObject.name}!");
            return false;
        }

        if (levelData.gameType != SupportedGameType)
        {
            Debug.LogError($"[CRITICAL] Wrong Game Type! Controller expects {SupportedGameType}, " +
                           $"but LevelData is {levelData.gameType}.");
            return false;
        }

        return true;
    }

    private void LoadBaseData()
    {
        // targetScore = dynamicTargetScore;
        pointsForCorrect = levelData.pointsForCorrect;
        pointsForWrong = levelData.pointsForWrong;
        useTimer = levelData.useTimer;
        levelTimeLimit = levelData.levelTimeLimit;
        maxMistakes = levelData.maxMistakes;
        
        if (levelData.possibleTargets != null && levelData.possibleTargets.Length > 0)
        {
            // Pick a random target from the list in LevelData
            int randomIndex = Random.Range(0, levelData.possibleTargets.Length);
            targetScore = levelData.possibleTargets[randomIndex];
        }
        else
        {
            targetScore = levelData.targetScore;
        }
        Debug.Log("The target score is " + targetScore);
    }

    // Override to load game-specific assets (SpawnItems, Questions) from LevelData.
    protected abstract void InitializeLevel();

    protected virtual void StartGame()
    {
        isGameActive = true;
        currentScore = 0;
        mistakeCount = 0;
        timeRemaining = levelTimeLimit;

        // Update the UI to display the current score
        OnScoreUpdated?.Invoke(currentScore);

        // Update the timer to show the remaining time
        if (useTimer) OnTimerUpdated?.Invoke(timeRemaining);
    }

    protected virtual void Timer()
    {
        timeRemaining -= Time.deltaTime;

        OnTimerUpdated?.Invoke(timeRemaining);

        if (timeRemaining <= 0)
        {
            EndLevel(false);
        }
    }

    protected virtual void HandleCorrectAnswer()
    {
        Debug.Log("Correct Answer!");

        currentScore += pointsForCorrect;

        OnScoreUpdated?.Invoke(currentScore);
    }

    protected virtual void HandleWrongAnswer()
    {
        Debug.Log("Wrong Answer!");
        mistakeCount++;

        if (pointsForWrong > 0)
        {
            // Add logic to handle NEGATIVE SCORES
            currentScore -= pointsForWrong;
        }

        if (maxMistakes > 0)
        {

            if (mistakeCount >= maxMistakes) EndLevel(false);
        }
    }

    protected virtual void CheckWinCondition()
    {
        if (currentScore >= targetScore)
        {
            EndLevel(true);
        }
    }

    protected virtual void EndLevel(bool isSuccess)
    {
        if (!isGameActive) return;

        isGameActive = false;

        if (isSuccess)
        {
            // Play Victory sound
            if (AudioManager.Instance != null) AudioManager.Instance.PlayLevelComplete();

            // Send the mistake count to GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ProcessLevelCompletion(levelData.levelID, mistakeCount, levelData);
            }
        }

        Debug.Log($"[BaseLevel] Level Over. Success: {isSuccess}. Mistakes: {mistakeCount}");

        // Tell UI to open the Win/Lose screen
        OnLevelEnded?.Invoke(isSuccess);
    }

}
