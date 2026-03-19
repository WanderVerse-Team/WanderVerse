using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LeaderboardRowUI : MonoBehaviour
{
    public TextMeshProUGUI rankText;
    public TextMeshProUGUI userNameText;
    public TextMeshProUGUI xpText;
    public GameObject highlightObject;

    public GameObject firstBadge;
    public GameObject secondBadge;
    public GameObject thirdBadge;

    public void Setup(int rank, string userName, int xp, bool isCurrentUser)
    {
        if (userNameText != null) userNameText.text = userName;
        if (xpText != null) xpText.text = xp.ToString();

        if (highlightObject != null)
            highlightObject.SetActive(isCurrentUser);

        if (firstBadge != null) firstBadge.SetActive(rank == 1);
        if (secondBadge != null) secondBadge.SetActive(rank == 2);
        if (thirdBadge != null) thirdBadge.SetActive(rank == 3);

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