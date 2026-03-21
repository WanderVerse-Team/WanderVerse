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
                subjectProgressBar.value = Mathf.Clamp01(totalProgress);
                
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
                DateTime lastReset = DateTimeOffset
                    .FromUnixTimeSeconds(data.lastDailyResetTimestamp)
                    .LocalDateTime.Date;

                DateTime today = DateTime.Now.Date;

                int difference = (today - lastReset).Days;

                if (difference == 0)
                    return data.loginStreak;

                if (difference == 1)
                    return data.loginStreak + 1;

                return 1; // reset streak
            }
        }
}