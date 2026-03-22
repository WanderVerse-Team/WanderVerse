using System;
using UnityEngine;
using UnityEngine.UI;          // Required for Slider
using TMPro;                   // Required for TextMeshProUGUI
using WanderVerse.Framework.Data;
using WanderVerse.Backend.Services;

namespace WanderVerse.Backend.UI
{
    public class DashboardUIController : MonoBehaviour
        {
            [Header("Top Profile")]
            public TextMeshProUGUI usernameText;
            public TextMeshProUGUI rankText; // Calculated based on XP
            public TextMeshProUGUI streakText;

            [Header("Progress Bars")]
            public Slider subjectProgressBar; // Overall Maths progress
            public TextMeshProUGUI map1PercentageText; 
            
            [Header("Badges")]
            public GameObject[] badgeIcons; // Seshani drags badge images here

            void OnEnable() 
            {
                UpdateDashboard();
            }

            public void UpdateDashboard()
            {
                var data = CloudSyncManager.Instance.CurrentData;
                if (data == null) return;

                // Basic Info
                usernameText.text = data.userName;
                streakText.text = "Login Streak: " + CalculateStreak(data) + " Days";
                rankText.text = GetRankName(data.xp);

                // Map & Subject Progress
                float totalProgress = CalculateSubjectProgress(data);
                subjectProgressBar.value = totalProgress;
                
                map1PercentageText.text = $"Island 1: {(totalProgress * 100):F0}%";

                // Badges (Simple Logic: If XP > 500, show Badge 0)
                badgeIcons[0].SetActive(data.xp > 500);
                badgeIcons[1].SetActive(data.currentLevel > 5);
            }

            private float CalculateSubjectProgress(PlayerData data)
            {
                if (data.levelProgress == null || data.levelProgress.Count == 0) return 0f;
                
                int completed = 0;
                foreach (var level in data.levelProgress)
                {
                    if (level.starsEarned > 0) completed++;
                }

                return (float)completed / data.levelProgress.Count; 
            }

            private string GetRankName(int xp)
            {
                if (xp < 100) return "Novice Explorer";
                if (xp < 500) return "Junior Wanderer";
                return "Master Pathfinder";
            }

            private int CalculateStreak(PlayerData data)
            {
                if (data == null) return 1;

                int savedStreak = Mathf.Max(1, data.loginStreak);
                if (data.lastDailyResetTimestamp <= 0) return savedStreak;

                DateTime lastResetDate = DateTimeOffset.FromUnixTimeSeconds(data.lastDailyResetTimestamp).LocalDateTime.Date;
                DateTime currentDate = DateTimeOffset.Now.Date;
                int dayGap = (currentDate - lastResetDate).Days;

                if (dayGap <= 0) return savedStreak;
                if (dayGap == 1) return savedStreak + 1;

                // Missed one or more days, so streak resets.
                return 1;
            }
        }
}