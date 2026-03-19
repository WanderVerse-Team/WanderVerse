using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WanderVerse.Backend.Services;

[Serializable]
public class LeaderboardEntry
{
    public string userID;
    public string userName;
    public int xp;
}

[Serializable]
public class LeaderboardResponse
{
    public List<LeaderboardEntry> leaderboard;
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
        LoadLeaderboard();
    }

    public void LoadLeaderboard()
    {
        ClearLeaderboard();

        if (statusText != null)
            statusText.text = "Loading leaderboard...";

        if (CloudSyncManager.Instance != null)
        {
            StartCoroutine(
                CloudSyncManager.Instance.FetchLeaderboard(OnLeaderboardSuccess, OnLeaderboardFailed)
            );
        }
        else
        {
            if (statusText != null)
                statusText.text = "CloudSyncManager not found.";
        }
    }

    private void OnLeaderboardSuccess(string json)
    {
        Debug.Log("Leaderboard JSON: " + json);

        LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(json);

        if (response == null || response.leaderboard == null || response.leaderboard.Count == 0)
        {
            if (statusText != null)
                statusText.text = "No leaderboard data found.";
            return;
        }

        string currentUserId = "";
        if (CloudSyncManager.Instance != null && CloudSyncManager.Instance.CurrentData != null)
        {
            currentUserId = CloudSyncManager.Instance.CurrentData.userID;
        }

        int currentPlayerRank = -1;
        LeaderboardEntry currentPlayerEntry = null;

        for (int i = 0; i < response.leaderboard.Count; i++)
        {
            int rank = i + 1;
            LeaderboardEntry entry = response.leaderboard[i];

            GameObject rowObj = Instantiate(leaderboardRowPrefab, contentParent);
            spawnedRows.Add(rowObj);

            LeaderboardRowUI rowUI = rowObj.GetComponent<LeaderboardRowUI>();
            bool isCurrentPlayer = entry.userID == currentUserId;

            rowUI.Setup(rank, entry.userName, entry.xp, isCurrentPlayer);

            if (isCurrentPlayer)
            {
                currentPlayerRank = rank;
                currentPlayerEntry = entry;
            }
        }

        if (myRankRowUI != null)
        {
            if (currentPlayerEntry != null)
            {
                myRankRowUI.Setup(currentPlayerRank, currentPlayerEntry.userName, currentPlayerEntry.xp, true);
            }
            else if (CloudSyncManager.Instance != null && CloudSyncManager.Instance.CurrentData != null)
            {
                var localData = CloudSyncManager.Instance.CurrentData;
                myRankRowUI.Setup(-1, localData.userName, localData.xp, true);
            }
        }

        if (statusText != null)
            statusText.text = "";
    }

    private void OnLeaderboardFailed(string error)
    {
        Debug.LogError("Leaderboard failed: " + error);

        if (statusText != null)
            statusText.text = "Failed to load leaderboard.";
    }

    private void ClearLeaderboard()
    {
        for (int i = 0; i < spawnedRows.Count; i++)
        {
            Destroy(spawnedRows[i]);
        }

        spawnedRows.Clear();
    }
}