using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WanderVerse.Backend.Services;

namespace WanderVerse.Backend.UI
{
    // JSON helper classes matching the backend response
    [Serializable]
    public class LeaderboardEntry
    {
        public string userName;
        public int xp;
    }

    [Serializable]
    public class CurrentUserEntry
    {
        public string userName;
        public int xp;
        public int rank;
    }

    [Serializable]
    public class LeaderboardResponse
    {
        public string status;
        public string source;
        public List<LeaderboardEntry> data;
        public CurrentUserEntry currentUser;
    }

    public class LeaderboardUIController : MonoBehaviour
    {
        [Header("Leaderboard Rows (assign 10 rows in Inspector)")]
        [Tooltip("Each row should have 3 TMP children: Rank, Name, XP")]
        public GameObject[] leaderboardRows;

        [Header("Current User Row (always visible at bottom)")]
        public TextMeshProUGUI currentUserRankText;
        public TextMeshProUGUI currentUserNameText;
        public TextMeshProUGUI currentUserXPText;
        public GameObject currentUserRow;

        [Header("Status")]
        public TextMeshProUGUI statusText;

        void OnEnable()
        {
            FetchAndDisplay();
        }

        public void FetchAndDisplay()
        {
            if (statusText != null) statusText.text = "Loading...";

            StartCoroutine(CloudSyncManager.Instance.FetchLeaderboard(
                onSuccess: (json) =>
                {
                    // Fix: Unity's JsonUtility can't parse null fields, so replace null with empty object
                    string safeJson = json.Replace("\"currentUser\":null", "\"currentUser\":{}");
                    LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(safeJson);

                    if (response == null || response.status != "SUCCESS")
                    {
                        if (statusText != null) statusText.text = "Failed to load leaderboard.";
                        return;
                    }

                    PopulateLeaderboard(response.data);
                    PopulateCurrentUser(response.currentUser);

                    if (statusText != null) statusText.text = "";
                },
                onFailure: (error) =>
                {
                    Debug.LogError("[LeaderboardUI] Error: " + error);
                    if (statusText != null) statusText.text = "Could not load leaderboard.";
                }
            ));
        }

        private void PopulateLeaderboard(List<LeaderboardEntry> entries)
        {
            if (entries == null) return;

            for (int i = 0; i < leaderboardRows.Length; i++)
            {
                if (i < entries.Count)
                {
                    leaderboardRows[i].SetActive(true);

                    // Each row expects 3 TMP children: [0] Rank, [1] Name, [2] XP
                    var texts = leaderboardRows[i].GetComponentsInChildren<TextMeshProUGUI>();
                    if (texts.Length >= 3)
                    {
                        texts[0].text = "#" + (i + 1);
                        texts[1].text = entries[i].userName;
                        texts[2].text = entries[i].xp.ToString() + " XP";
                    }
                }
                else
                {
                    // Hide unused rows
                    leaderboardRows[i].SetActive(false);
                }
            }
        }

        private void PopulateCurrentUser(CurrentUserEntry user)
        {
            if (currentUserRow == null) return;

            if (user == null || string.IsNullOrEmpty(user.userName))
            {
                currentUserRow.SetActive(false);
                return;
            }

            currentUserRow.SetActive(true);

            if (currentUserRankText != null) currentUserRankText.text = "#" + user.rank;
            if (currentUserNameText != null) currentUserNameText.text = user.userName;
            if (currentUserXPText != null) currentUserXPText.text = user.xp.ToString() + " XP";
        }
    }
}
