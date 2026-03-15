using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TopNavBarController : MonoBehaviour
{
    [Header("Tab Buttons")]
    public Button Dashboard_Btn;
    public Button World_Btn;
    public Button Leaderboard_Btn;

    [Header("Tab Images")]
    public Image dashboardImage;
    public Image worldImage;
    public Image leaderboardImage;

    [Header("Dashboard Sprites")]
    public Sprite dashboardBlack;
    public Sprite dashboardOrange;

    [Header("World Sprites")]
    public Sprite worldBlack;
    public Sprite worldOrange;

    [Header("Leaderboard Sprites")]
    public Sprite leaderboardBlack;
    public Sprite leaderboardOrange;

    [Header("Scene Names")]
    public string dashboardSceneName = "Scene_Dashboard";
    public string worldSceneName = "Scene_WorldMap";
    public string leaderboardSceneName = "Scene_Leaderboard";

    void Start()
    {
        UpdateTabVisuals();

        Dashboard_Btn.onClick.AddListener(() => LoadScene(dashboardSceneName));
        World_Btn.onClick.AddListener(() => LoadScene(worldSceneName));
        Leaderboard_Btn.onClick.AddListener(() => LoadScene(leaderboardSceneName));
    }

    void UpdateTabVisuals()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        dashboardImage.sprite = currentScene == dashboardSceneName ? dashboardOrange : dashboardBlack;
        worldImage.sprite = currentScene == worldSceneName ? worldOrange : worldBlack;
        leaderboardImage.sprite = currentScene == leaderboardSceneName ? leaderboardOrange : leaderboardBlack;
    }

    void LoadScene(string sceneName)
    {
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
