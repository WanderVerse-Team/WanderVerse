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

    private void Awake()
    {
        restartButton.onClick.AddListener(RestartLevel);
        nextLevelButton.onClick.AddListener(LoadNextLevel);
        menuButton.onClick.AddListener(ReturnToMenu);

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
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelCompleted += HandleLevelCompleted;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnLevelCompleted -= HandleLevelCompleted;
        }
    }

    private void HandleLevelCompleted(string levelID, int currentRunScore, int xpAdded, int stars, bool isNewBest)
    {
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
