using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WanderVerse.Backend.Services;
using WanderVerse.Framework.Data;

[Serializable]
public class LeaderboardEntry
{
    public string userID;
    public string userName;
    public int xp;
}

[Serializable]
public class LeaderboardArrayWrapper
{
    public LeaderboardEntry[] items;
}

[Serializable]
public class LeaderboardResponseWrapper
{
    public LeaderboardEntry[] leaderboard;
    public LeaderboardEntry[] entries;
    public LeaderboardEntry[] data;
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

    [Header("Settings")]
    public bool showOnlyTop10 = true;
    public int maxRows = 10;

    private readonly List<GameObject> spawnedRows = new List<GameObject>();

    private void OnEnable()
    {
        Debug.Log("[LeaderboardUI] OnEnable called");

        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(LoadLeaderboardRoutine());
        }
    }

    public IEnumerator LoadLeaderboardRoutine()
    {
        ClearLeaderboard();

        if (statusText != null)
            statusText.text = "Loading leaderboard...";

        if (CloudSyncManager.Instance == null)
        {
            Debug.LogError("[LeaderboardUI] CloudSyncManager is missing.");
            if (statusText != null)
                statusText.text = "Leaderboard unavailable.";
            yield break;
        }

        bool requestFinished = false;
        string responseJson = null;
        string errorMessage = null;

        yield return StartCoroutine(
            CloudSyncManager.Instance.FetchLeaderboard(
                onSuccess: (json) =>
                {
                    responseJson = json;
                    requestFinished = true;
                },
                onFailure: (error) =>
                {
                    errorMessage = error;
                    requestFinished = true;
                }
            )
        );

        yield return new WaitUntil(() => requestFinished);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            Debug.LogError("[LeaderboardUI] Backend error: " + errorMessage);

            if (statusText != null)
                statusText.text = "Failed to load leaderboard.";

            SetupMyRankFallback();
            yield break;
        }

        if (string.IsNullOrEmpty(responseJson))
        {
            Debug.LogWarning("[LeaderboardUI] Empty leaderboard response.");

            if (statusText != null)
                statusText.text = "No leaderboard data found.";

            SetupMyRankFallback();
            yield break;
        }

        List<LeaderboardEntry> entries = ParseLeaderboardJson(responseJson);

        if (entries == null || entries.Count == 0)
        {
            Debug.LogWarning("[LeaderboardUI] No valid entries parsed from backend JSON.");
            Debug.Log("[LeaderboardUI] Raw JSON: " + responseJson);

            if (statusText != null)
                statusText.text = "No leaderboard entries.";

            SetupMyRankFallback();
            yield break;
        }

        // Sort highest XP first, just in case backend does not sort it already
        entries.Sort((a, b) => b.xp.CompareTo(a.xp));

        string currentUserId = GetCurrentUserId();

        int currentUserRank = -1;
        LeaderboardEntry currentUserEntry = null;

        for (int i = 0; i < entries.Count; i++)
        {
            if (!string.IsNullOrEmpty(currentUserId) && entries[i].userID == currentUserId)
            {
                currentUserRank = i + 1;
                currentUserEntry = entries[i];
                break;
            }
        }

        int rowsToShow = entries.Count;
        if (showOnlyTop10)
            rowsToShow = Mathf.Min(maxRows, entries.Count);

        for (int i = 0; i < rowsToShow; i++)
        {
            int rank = i + 1;
            LeaderboardEntry entry = entries[i];

            GameObject rowObj = Instantiate(leaderboardRowPrefab, contentParent);
            spawnedRows.Add(rowObj);

            RectTransform rowRect = rowObj.GetComponent<RectTransform>();
            if (rowRect != null)
            {
                rowRect.localScale = Vector3.one;
                rowRect.localRotation = Quaternion.identity;
                rowRect.anchoredPosition = Vector2.zero;
            }

            LeaderboardRowUI rowUI = rowObj.GetComponent<LeaderboardRowUI>();
            if (rowUI != null)
            {
                bool isCurrentPlayer = !string.IsNullOrEmpty(currentUserId) && entry.userID == currentUserId;
                rowUI.Setup(rank, entry.userName, entry.xp, isCurrentPlayer);
            }
        }

        // Fill the separate "My Rank" row
        if (myRankRowUI != null)
        {
            if (currentUserEntry != null)
            {
                myRankRowUI.Setup(currentUserRank, currentUserEntry.userName, currentUserEntry.xp, true);
            }
            else
            {
                SetupMyRankFallback();
            }
        }

        if (statusText != null)
            statusText.text = "";
    }

    private List<LeaderboardEntry> ParseLeaderboardJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new List<LeaderboardEntry>();

        json = json.Trim();

        try
        {
            // Case 1: raw JSON array
            // Example: [ {...}, {...} ]
            if (json.StartsWith("["))
            {
                string wrappedJson = "{\"items\":" + json + "}";
                LeaderboardArrayWrapper arrayWrapper = JsonUtility.FromJson<LeaderboardArrayWrapper>(wrappedJson);

                if (arrayWrapper != null && arrayWrapper.items != null)
                    return new List<LeaderboardEntry>(arrayWrapper.items);
            }

            // Case 2: wrapped JSON object
            // Example: { "leaderboard": [...] }
            // or      { "entries": [...] }
            // or      { "data": [...] }
            LeaderboardResponseWrapper wrapper = JsonUtility.FromJson<LeaderboardResponseWrapper>(json);

            if (wrapper != null)
            {
                if (wrapper.leaderboard != null && wrapper.leaderboard.Length > 0)
                    return new List<LeaderboardEntry>(wrapper.leaderboard);

                if (wrapper.entries != null && wrapper.entries.Length > 0)
                    return new List<LeaderboardEntry>(wrapper.entries);

                if (wrapper.data != null && wrapper.data.Length > 0)
                    return new List<LeaderboardEntry>(wrapper.data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[LeaderboardUI] JSON parse error: " + e.Message);
        }

        return new List<LeaderboardEntry>();
    }

    private string GetCurrentUserId()
    {
        if (CloudSyncManager.Instance != null && CloudSyncManager.Instance.CurrentData != null)
            return CloudSyncManager.Instance.CurrentData.userID;

        return string.Empty;
    }

    private void SetupMyRankFallback()
    {
        if (myRankRowUI == null)
            return;

        PlayerData currentData = CloudSyncManager.Instance != null ? CloudSyncManager.Instance.CurrentData : null;

        if (currentData != null)
        {
            myRankRowUI.Setup(
                0,
                currentData.userName,
                currentData.xp,
                true
            );
        }
        else
        {
            myRankRowUI.Setup(
                0,
                "You",
                0,
                true
            );
        }
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

    private void OnDisable()
    {
        ClearLeaderboard();
    }
}