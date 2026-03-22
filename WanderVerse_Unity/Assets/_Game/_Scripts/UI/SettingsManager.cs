using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("--- UI References ---")]
    [Tooltip("Drag your single Settings Panel background here")]
    public GameObject settingsPanel;
    
    [Tooltip("Drag the Return to Menu button here")]
    public GameObject returnToMenuButton;
    
    [Tooltip("Drag the Quit Game button here")]
    public GameObject quitGameButton;

    [Tooltip("Drag the Return to Game button here")]
    public GameObject returnToGameButton;

    [Tooltip("Drag your single Master Volume Slider here")]
    public Slider masterVolumeSlider;

    [Header("--- Settings ---")]
    [Tooltip("Type the EXACT name of your Main Menu scene here")]
    public string mainMenuSceneName = "MainMenu"; // Update this to your actual menu scene name

    private bool isGamePaused = false;

    private void Start()
    {
        // 1. Hide the panel when the scene loads
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // 2. Decide which button to show based on the current scene
        CheckCurrentScene();

        // 3. Set up the slider and connect it to the AudioManager via code
        InitializeSlider();
    }

    private void CheckCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // if (currentScene == mainMenuSceneName)
        // {
        //     // We are in the Main Menu: Show Quit, Hide Return
        //     if (returnToMenuButton != null) returnToMenuButton.SetActive(false);
        //     if (quitGameButton != null) quitGameButton.SetActive(true);
        // }
        // else
        // {
        //     // We are in a Game Level: Show Return, Hide Quit
        //     if (returnToMenuButton != null) returnToMenuButton.SetActive(true);
        //     if (quitGameButton != null) quitGameButton.SetActive(false);
        // }
    }

    private void InitializeSlider()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found in scene!");
            return;
        }

        if (masterVolumeSlider != null)
        {
            // Convert saved Decibels back to a slider value (0.0001 to 1)
            float savedVolume = PlayerPrefs.GetFloat("MasterVol", 0f);
            masterVolumeSlider.value = Mathf.Pow(10f, savedVolume / 20f);

            // Connect the slider to the AudioManager automatically
            masterVolumeSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        }
    }

    // ==========================================
    // BUTTON FUNCTIONS
    // ==========================================

    public void OpenSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        
        if (settingsPanel != null) settingsPanel.SetActive(true);

        // Only freeze time if we are in a game level
        if (SceneManager.GetActiveScene().name != mainMenuSceneName)
        {
            Time.timeScale = 0f; 
            isGamePaused = true;
        }
    }

    public void CloseSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        
        if (settingsPanel != null) settingsPanel.SetActive(false);

        // Unfreeze the game if it was paused
        if (isGamePaused)
        {
            Time.timeScale = 1f; 
            isGamePaused = false;
        }
    }

    public void ReturnToMenu()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        
        Time.timeScale = 1f; //Unfreeze time before loading a new scene
        SceneManager.LoadScene(mainMenuSceneName);
        Screen.orientation = ScreenOrientation.Portrait;
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        
        Debug.Log("Game is quitting...");
        Application.Quit(); 
    }
}