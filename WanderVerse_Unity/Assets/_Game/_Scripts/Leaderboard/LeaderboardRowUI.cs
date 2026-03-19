using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardRowUI : MonoBehaviour
{
    [Header("Basic UI")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI usernameText;
    public TextMeshProUGUI scoreText;
    public Image avatarImage;

    [Header("Top 3 Visuals")]
    public GameObject top1Badge;
    public GameObject top2Badge;
    public GameObject top3Badge;
    public Image rowBackground;

    [Header("Optional Backgrounds")]
    public Sprite normalBg;
    public Sprite top1Bg;
    public Sprite top2Bg;
    public Sprite top3Bg;
    public Sprite currentPlayerBg;

    public void Setup(int rank, string username, int score, bool isCurrentPlayer = false)
    {
        rankText.text = rank.ToString();
        usernameText.text = username;
        scoreText.text = score.ToString();

        if (top1Badge != null) top1Badge.SetActive(rank == 1);
        if (top2Badge != null) top2Badge.SetActive(rank == 2);
        if (top3Badge != null) top3Badge.SetActive(rank == 3);

        if (rowBackground != null)
        {
            if (isCurrentPlayer && currentPlayerBg != null)
                rowBackground.sprite = currentPlayerBg;
            else if (rank == 1 && top1Bg != null)
                rowBackground.sprite = top1Bg;
            else if (rank == 2 && top2Bg != null)
                rowBackground.sprite = top2Bg;
            else if (rank == 3 && top3Bg != null)
                rowBackground.sprite = top3Bg;
            else if (normalBg != null)
                rowBackground.sprite = normalBg;
        }
    }
}
