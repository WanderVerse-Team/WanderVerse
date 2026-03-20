using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("--- Main Menu Settings Panel ---")]
    [Tooltip("Drag the Settings Panel used ONLY in the Main Menu here")]
    public GameObject mainMenuSettingsPanel;
    public Slider mainMenuMasterSlider;

    [Header("--- In-Game Settings Panel ---")]
    [Tooltip("Drag the Settings Panel used ONLY during gameplay here")]
    public GameObject inGameSettingsPanel;
    public Slider inGameMasterSlider;

    [Header("--- Settings ---")]
    [Tooltip("Type the EXACT name of your Main Menu scene here")]
    public string mainMenuSceneName = "MainMenu"; 

    private bool isGamePaused = false;

    private void Start()
    {
        // 1. Ensure both panels are hidden when the scene starts
        // if (mainMenuSettingsPanel != null) mainMenuSettingsPanel.SetActive(false);
        // if (inGameSettingsPanel != null) inGameSettingsPanel.SetActive(false);

        // 2. Setup the sliders
        InitializeSliders();
    }

    private void InitializeSliders()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager not found in scene!");
            return;
        }

        float savedVolume = DBToSliderValue(PlayerPrefs.GetFloat("MasterVol", 0f));

        // Setup Main Menu Slider
        if (mainMenuMasterSlider != null)
        {
            mainMenuMasterSlider.value = savedVolume;
            mainMenuMasterSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        }

        // Setup In-Game Slider
        if (inGameMasterSlider != null)
        {
            inGameMasterSlider.value = savedVolume;
            inGameMasterSlider.onValueChanged.AddListener(AudioManager.Instance.SetMasterVolume);
        }
    }

    private float DBToSliderValue(float dbValue)
    {
        return Mathf.Pow(10f, dbValue / 20f);
    }

    // ==========================================
    // BUTTON FUNCTIONS
    // ==========================================

    public void OpenSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        string currentScene = SceneManager.GetActiveScene().name;

        // Check where we are and open the correct panel
        if (currentScene == mainMenuSceneName)
        {
            if (mainMenuSettingsPanel != null) mainMenuSettingsPanel.SetActive(true);
        }
        else
        {
            if (inGameSettingsPanel != null) inGameSettingsPanel.SetActive(true);
            
            // Freeze game time
            Time.timeScale = 0f; 
            isGamePaused = true;
        }
    }

    public void CloseSettings()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        // Hide both panels just to be safe
        if (mainMenuSettingsPanel != null) mainMenuSettingsPanel.SetActive(false);
        if (inGameSettingsPanel != null) inGameSettingsPanel.SetActive(false);

        // Unfreeze the game if it was paused
        if (isGamePaused)
        {
            Time.timeScale = 1f; 
            isGamePaused = false;
        }
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f; // CRITICAL: Unfreeze time before leaving!
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();
        Debug.Log("Game is quitting...");
        Application.Quit(); 
    }
}