using UnityEngine;
using TMPro;

public class LeaderboardRowUI : MonoBehaviour
{
    [Header("Text")]
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI userNameText;
    public TextMeshProUGUI xpText;

    [Header("Highlight")]
    public GameObject highlightObject;

    [Header("Top 3 Badges")]
    public GameObject firstBadge;
    public GameObject secondBadge;
    public GameObject thirdBadge;

    public void Setup(int rank, string userName, int xp, bool isCurrentUser)
    {
        if (userNameText != null)
            userNameText.text = userName;

        if (xpText != null)
            xpText.text = xp.ToString();

        if (highlightObject != null)
            highlightObject.SetActive(isCurrentUser);

        if (firstBadge != null)
            firstBadge.SetActive(rank == 1);

        if (secondBadge != null)
            secondBadge.SetActive(rank == 2);

        if (thirdBadge != null)
            thirdBadge.SetActive(rank == 3);

        if (rankText != null)
        {
            if (rank == 1 || rank == 2 || rank == 3)
            {
                rankText.gameObject.SetActive(false);
            }
            else
            {
                rankText.gameObject.SetActive(true);
                rankText.text = rank.ToString();
            }
        }
    }
}