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

    [Tooltip("Drag the Otter Image GameObject here")]
    [SerializeField] private RectTransform otterTransform;

    [Tooltip("Drag the Main_Window Image here")]
    [SerializeField] private RectTransform mainWindowTransform;

    [Tooltip("Drag the semi-transparent black background here")]
    [SerializeField] private GameObject backgroundDimmer;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button menuButton;

    [Header("Effects")]
    //[Tooltip("Drag the Confetti_Particles here")]
    //[SerializeField] private ParticleSystem confettiParticles;

    [Tooltip("Drag the Confetti here")]
    [SerializeField] private ParticleSystem confettiParticles;

    [Tooltip("Drag the Fireworks here")]
    [SerializeField] private ParticleSystem fireworksParticles;

    private void Awake()
    {
        Canvas myCanvas = GetComponent<Canvas>();
        if (myCanvas != null)
        {
            myCanvas.worldCamera = Camera.main;

            myCanvas.planeDistance = 1f;
        }

        // Setup Button Listeners
        restartButton.onClick.AddListener(RestartLevel);
        nextLevelButton.onClick.AddListener(LoadNextLevel);
        menuButton.onClick.AddListener(ReturnToMenu);

        // Hide visuals initially
        if (mainWindowTransform != null) mainWindowTransform.gameObject.SetActive(false);
        if (backgroundDimmer != null) backgroundDimmer.SetActive(false);
        if (otterTransform != null) otterTransform.localScale = Vector3.zero;
        if (xpText != null) xpText.transform.localScale = Vector3.zero;

        //if (confettiParticles != null) confettiParticles.Stop();
        if (confettiParticles != null) confettiParticles.Stop();
        if (fireworksParticles != null) fireworksParticles.Stop();
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
        // Turn the visuals ON
        if (backgroundDimmer != null) backgroundDimmer.SetActive(true);
        if (mainWindowTransform != null)
        {
            mainWindowTransform.gameObject.SetActive(true);
            mainWindowTransform.localScale = Vector3.zero;
        }
        if (otterTransform != null) otterTransform.gameObject.SetActive(true);

        // Pause the game world
        Time.timeScale = 0f;

        scoreText.text = $"Score: {currentRunScore}";

        if (xpAdded > 0)
        {
            if (isNewBest) xpText.text = $"+{xpAdded} XP (New Best!)";
            else xpText.text = $"+{xpAdded} XP (Perfect Revision!)";
        }
        else
        {
            xpText.text = "";
        }

        StartCoroutine(AnimateWinScreen(stars, xpAdded));
    }

    private IEnumerator AnimateWinScreen(int starsAchieved, int xpAdded)
    {
        if (mainWindowTransform != null)
        {
            mainWindowTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        }

        //if (confettiParticles != null)
        //{
        //    confettiParticles.gameObject.SetActive(true);
        //    confettiParticles.Play();
        //}
        if (confettiParticles != null) confettiParticles.Play();
        if (fireworksParticles != null) fireworksParticles.Play();

        yield return new WaitForSecondsRealtime(0.25f);
        if (otterTransform != null)
        {
            otterTransform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
            // AudioManager.Instance.PlaySFX("OtterSound");
        }

        yield return new WaitForSecondsRealtime(0.25f);

        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < starsAchieved)
            {
                starImages[i].transform.localScale = Vector3.zero;
                starImages[i].sprite = fullStarSprite;
                starImages[i].transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);

                // AudioManager.Instance.PlaySFX("StarPopSound");
                yield return new WaitForSecondsRealtime(0.3f);
            }
        }

        if (xpAdded > 0 && xpText != null)
        {
            yield return new WaitForSecondsRealtime(0.2f);
            xpText.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;
        if (AudioManager.Instance != null) AudioManager.Instance.StopAllAudio();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void LoadNextLevel()
    {
        Time.timeScale = 1f;
        if (AudioManager.Instance != null) AudioManager.Instance.StopAllAudio();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        if (AudioManager.Instance != null) AudioManager.Instance.StopAllAudio();
        SceneManager.LoadScene("Golem_Game_Map");
    }
}