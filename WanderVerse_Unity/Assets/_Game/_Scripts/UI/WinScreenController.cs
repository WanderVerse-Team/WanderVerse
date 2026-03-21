using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;

public class WinScreenController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI xpText;
    [SerializeField] private Image[] starImages;
    [SerializeField] private Sprite fullStarSprite;

    [Tooltip("Drag the Main_Window Image here")]
    [SerializeField] private RectTransform mainWindowTransform;

    [Tooltip("Drag the semi-transparent black background here")]
    [SerializeField] private GameObject backgroundDimmer;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button menuButton;

    private BaseLevelController activeLevelController;
    private bool hasShownWin;
    private CanvasGroup rootCanvasGroup;

    private void Awake()
    {
        if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
        if (nextLevelButton != null) nextLevelButton.onClick.AddListener(LoadNextLevel);
        if (menuButton != null) menuButton.onClick.AddListener(ReturnToMenu);

        rootCanvasGroup = GetComponent<CanvasGroup>();
        if (rootCanvasGroup == null)
            rootCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetCanvasInteractivity(false);

        // Hide visuals initially, but KEEP this root script active so it can listen!
        if (mainWindowTransform != null) mainWindowTransform.gameObject.SetActive(false);
        if (backgroundDimmer != null) backgroundDimmer.SetActive(false);

        foreach (Image star in starImages)
        {
            star.transform.localScale = Vector3.zero;
        }
    }

    private void OnEnable()
    {
        hasShownWin = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelCompleted += HandleLevelCompleted;
        }

        activeLevelController = FindAnyObjectByType<BaseLevelController>();
        if (activeLevelController != null)
            activeLevelController.OnLevelEnded += HandleLevelEndedFallback;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
        }

        if (activeLevelController != null)
        {
            activeLevelController.OnLevelEnded -= HandleLevelEndedFallback;
            activeLevelController = null;
        }
    }

    private void HandleLevelCompleted(string levelID, int currentRunScore, int xpAdded, int stars, bool isNewBest)
    {
        ShowWinUI(currentRunScore, xpAdded, stars, isNewBest);
    }

    private void HandleLevelEndedFallback(bool isSuccess)
    {
        if (!isSuccess || hasShownWin) return;

        int score = activeLevelController != null ? activeLevelController.CurrentScore : 0;
        int mistakes = activeLevelController != null ? activeLevelController.MistakeCount : 0;
        LevelData levelData = activeLevelController != null ? activeLevelController.CurrentLevelData : null;

        int stars = 1;
        if (levelData != null)
        {
            if (mistakes <= levelData.maxMistakesFor3Stars) stars = 3;
            else if (mistakes <= levelData.maxMistakesFor2Stars) stars = 2;
        }

        ShowWinUI(score, 0, stars, false);
    }

    private void ShowWinUI(int currentRunScore, int xpAdded, int stars, bool isNewBest)
    {
        if (hasShownWin) return;
        hasShownWin = true;

        SetCanvasInteractivity(true);

        // 1. Turn the visuals ON
        if (backgroundDimmer != null) backgroundDimmer.SetActive(true);
        if (mainWindowTransform != null)
        {
            mainWindowTransform.gameObject.SetActive(true);
            mainWindowTransform.localScale = Vector3.zero; // Reset scale for the pop
        }

        // 2. Pause the game world
        Time.timeScale = 0f;

        // 3. Set Text
        scoreText.text = currentRunScore.ToString();

        if (isNewBest)
        {
            xpText.text = $"+{xpAdded} XP (New Best!)";
        }
        else if (xpAdded > 0)
        {
            xpText.text = $"+{xpAdded} XP (Perfect Revision!)";
        }
        else
        {
            xpText.text = "+0 XP";
        }

        // 4. Animate!
        StartCoroutine(AnimateWinScreen(stars));
    }

    private void SetCanvasInteractivity(bool enabled)
    {
        if (rootCanvasGroup == null) return;

        rootCanvasGroup.interactable = enabled;
        rootCanvasGroup.blocksRaycasts = enabled;
    }

    private IEnumerator AnimateWinScreen(int starsAchieved)
    {
        if (mainWindowTransform != null)
        {
            mainWindowTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        yield return new WaitForSecondsRealtime(0.5f);

        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < starsAchieved)
            {
                starImages[i].sprite = fullStarSprite;
                starImages[i].transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

                // AudioManager.Instance.PlaySFX("VictorySound");
                yield return new WaitForSecondsRealtime(0.3f);
            }
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Golem_Game_Map");
    }
}
