using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class BaseLevelController : MonoBehaviour
{
    [Header("--- LEVEL DATA ---")]
    [Tooltip("Drag the LevelData ScriptableObject here")]
    [SerializeField] protected LevelData levelData;

    [Header("--- MASCOT TUTORIAL---")]
    public GameObject tutorialPanel;
    public TMP_Text dialogueText;
    public Image mascotImage;
    [TextArea(2, 4)] public string[] mascotMessages;
    public AudioClip[] mascotVoices;
    public Sprite[] mascotPoses;

    private int currentMessageIndex = 0;

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

    public int CurrentScore => currentScore;
    public int MistakeCount => mistakeCount;
    public LevelData CurrentLevelData => levelData;

    protected virtual void Awake()
    {
        // If you need to initialize any level specific internal variables, do it here
        SpawnWinScreen();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        if (!ValidateLevelData()) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentLevelData(levelData);
            Debug.Log($"[BaseLevelController] Registered LevelData: {levelData.levelTitle}");
        }
        else
        {
            Debug.LogWarning("[BaseLevelController] No GameManager found! Starting in DEBUG mode.");
        }

        LoadBaseData();
        InitializeLevel();

        // Play background music
        if (AudioManager.Instance != null && levelData.backgroundMusic != null) 
        {
            AudioManager.Instance.PlayMusic(levelData.backgroundMusic );
        }

        if (GameManager.Instance != null)
        {
            Debug.Log($"[BaseLevelController] GameManager found! Starting the {gameObject.name}...");
        }
        else
        {
            Debug.LogWarning("[BaseLevelController] No GameManager found! Starting in DEBUG mode (using Inspector data).");
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
        isGameActive = false; // Make sure the game is paused!
        
        // If we have tutorial messages, show them
        if (tutorialPanel != null && mascotMessages != null && mascotMessages.Length > 0)
        {
            currentMessageIndex = 0;
            tutorialPanel.SetActive(true);
            ShowCurrentTutorialMessage();
        }
        else
        {
            // If no tutorial is set up, skip straight to the game
            BeginLevel();
        }
    }

    private void ShowCurrentTutorialMessage()
    {
        // 1. Text
        if (dialogueText != null && mascotMessages.Length > currentMessageIndex) 
        {
            // Get the raw text from the Inspector
            string rawMessage = mascotMessages[currentMessageIndex];

            // --- NEW: Replace our special placeholders with the actual numbers! ---
            string formattedMessage = rawMessage.Replace("{TARGET}", targetScore.ToString());
            formattedMessage = formattedMessage.Replace("{TIME}", levelTimeLimit.ToString());
            formattedMessage = formattedMessage.Replace("{MISTAKES}", maxMistakes.ToString());

            // Show the formatted text on the screen
            dialogueText.text = formattedMessage;
        }

        // 2. Audio
        if (AudioManager.Instance != null && mascotVoices != null && mascotVoices.Length > currentMessageIndex)
        {
            AudioClip voiceClip = mascotVoices[currentMessageIndex];
            if (voiceClip != null) AudioManager.Instance.PlayVoiceover(voiceClip);
        }

        // 3. Looping Poses
        if (mascotImage != null && mascotPoses != null && mascotPoses.Length > 0)
        {
            int loopedIndex = currentMessageIndex % mascotPoses.Length;
            if (mascotPoses[loopedIndex] != null) mascotImage.sprite = mascotPoses[loopedIndex];
        }
    }

    // Call this from a full-screen invisible Button on your Tutorial Panel!
    public void AdvanceTutorial()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        
        currentMessageIndex++;

        if (currentMessageIndex < mascotMessages.Length)
        {
            ShowCurrentTutorialMessage();
        }
        else
        {
            // Tutorial is over!
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            BeginLevel(); 
        }
    }

    // This runs AFTER the tutorial is finished.
    // Child classes (like TreasurePacker) can override this to spawn their specific items!
    protected virtual void BeginLevel()
    {
        isGameActive = true;
        currentScore = 0;
        mistakeCount = 0;
        timeRemaining = levelTimeLimit;

        OnScoreUpdated?.Invoke(currentScore);
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

    private void SpawnWinScreen()
    {
        if (FindAnyObjectByType<WinScreenController>() != null) return;

        // Load the prefab directly from the Resources folder
        GameObject winScreenPrefab = Resources.Load<GameObject>("WinScreen_Canvas");

        // Spawn it into the hierarchy
        if (winScreenPrefab != null)
        {
            Instantiate(winScreenPrefab);
            Debug.Log("[BaseLevelController] Successfully spawned the Win Screen UI.");
        }
        else
        {
            Debug.LogError("[BaseLevelController] Could not find 'WinScreen_Canvas' in a Resources folder!");
        }
    }

}
