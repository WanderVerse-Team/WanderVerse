using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Serializable]
public class LeaderboardEntry
{
    public string userID;
    public string userName;
    public int xp;
}

public class LeaderboardUIController : MonoBehaviour
{
    [Header("Main List")]
    public Transform contentParent;
    public GameObject leaderboardRowPrefab;

    [Header("My Rank Container")]
    public LeaderboardRowUI myRankRowUI;

    [Header("Optional UI")]
    public TextMeshProUGUI statusText;

    private List<GameObject> spawnedRows = new List<GameObject>();

    private void OnEnable()
    {
        Debug.Log("LeaderboardUIController OnEnable is running");
        LoadTestLeaderboard();
    }

    public void LoadTestLeaderboard()
    {
        ClearLeaderboard();

        if (statusText != null)
            statusText.text = "Loading test leaderboard...";

        List<LeaderboardEntry> testEntries = new List<LeaderboardEntry>
        {
            new LeaderboardEntry { userID = "u1", userName = "Seshani", xp = 1500 },
            new LeaderboardEntry { userID = "u2", userName = "Nimal", xp = 1300 },
            new LeaderboardEntry { userID = "u3", userName = "Asha", xp = 1200 },
            new LeaderboardEntry { userID = "u4", userName = "Kavi", xp = 1000 },
            new LeaderboardEntry { userID = "u5", userName = "Mira", xp = 900 }
        };

        for (int i = 0; i < testEntries.Count; i++)
        {
            int rank = i + 1;
            LeaderboardEntry entry = testEntries[i];

            GameObject rowObj = Instantiate(leaderboardRowPrefab, contentParent);
            spawnedRows.Add(rowObj);

            LeaderboardRowUI rowUI = rowObj.GetComponent<LeaderboardRowUI>();
            bool isCurrentPlayer = entry.userID == "u4";

            rowUI.Setup(rank, entry.userName, entry.xp, isCurrentPlayer);
        }

        if (myRankRowUI != null)
        {
            myRankRowUI.Setup(4, "Kavi", 1000, true);
        }

        if (statusText != null)
            statusText.text = "";
    }

    private void ClearLeaderboard()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            if (spawnedRows[i] != null)
                Destroy(spawnedRows[i]);
        }

        spawnedRows.Clear();
    }
}