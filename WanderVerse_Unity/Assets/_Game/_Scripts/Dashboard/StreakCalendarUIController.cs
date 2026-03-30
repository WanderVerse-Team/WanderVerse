using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WanderVerse.Backend.Services;
using WanderVerse.Framework.Data;

public class StreakCalendarUIController : MonoBehaviour
{
    [Header("Top UI")]
    public TextMeshProUGUI streakCountText;
    public TextMeshProUGUI resetTimerText;

    [Header("Calendar")]
    public Transform daysContainer;
    public GameObject streakDayItemPrefab;

    [Header("Settings")]
    public int totalDaysToShow = 7;

    private readonly List<GameObject> spawnedItems = new List<GameObject>();

    private void OnEnable()
    {
        RefreshUI();
    }

    private void Update()
    {
        if (resetTimerText != null && EnergyManager.Instance != null)
        {
            resetTimerText.text = "Resets in " + EnergyManager.Instance.GetTimeUntilMidnight();
        }
    }

    public void RefreshUI()
    {
        ClearItems();

        if (CloudSyncManager.Instance == null || CloudSyncManager.Instance.CurrentData == null)
            return;

        PlayerData data = CloudSyncManager.Instance.CurrentData;

        int streak = Mathf.Max(0, data.loginStreak);

        if (streakCountText != null)
        {
            if (streak == 1)
                streakCountText.text = "1 Day Streak";
            else
                streakCountText.text = streak + " Day Streak";
        }

        DateTime today = DateTime.Now.Date;

        // Get Monday of the current week
        int diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime startOfWeek = today.AddDays(-diff);

        for (int i = 0; i < totalDaysToShow; i++)
        {
            DateTime day = startOfWeek.AddDays(i);

            bool isToday = day == today;

            int daysAgo = (today - day).Days;
            bool isCompleted = daysAgo >= 0 && daysAgo < streak;

            GameObject item = Instantiate(streakDayItemPrefab, daysContainer);
            spawnedItems.Add(item);

            StreakDayItemUI itemUI = item.GetComponent<StreakDayItemUI>();
            if (itemUI != null)
            {
                itemUI.Setup(day.ToString("ddd"), isCompleted, isToday);
            }
        }
    }

    private void ClearItems()
    {
        for (int i = 0; i < spawnedItems.Count; i++)
        {
            if (spawnedItems[i] != null)
                Destroy(spawnedItems[i]);
        }

        spawnedItems.Clear();
    }
}