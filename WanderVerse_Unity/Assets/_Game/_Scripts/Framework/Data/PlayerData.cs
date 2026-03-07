using System;
using System.Collections.Generic;

namespace WanderVerse.Framework.Data
{
    [Serializable]
    public class LevelTracker
    {
        public string levelID;    
        public int attempts;      
        public int starsEarned; // For Visuals
        public int highScore;   
        public bool isUnlocked;
    }

    [Serializable]
    public class PlayerData
    {
        public string userID;
        public string userName = "Explorer";
        public string lastUpdated; 

    
        public int xp;           
        public int currentLevel = 1; 

        
        public int energy = 6;
        public int maxEnergy = 6;
        public long lastRechargeTimestamp;   
        public long lastDailyResetTimestamp; 

        
        public int selectedGrade = 3;
        public string selectedSubject = "Maths";
        public string selectedLanguage = "Sinhala"; 
        public bool hasCompletedOnboarding = false;

        
        public List<LevelTracker> levelProgress = new List<LevelTracker>();

        public PlayerData()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this.lastRechargeTimestamp = now;
            this.lastDailyResetTimestamp = now;
            MarkAsUpdated();
        }

        public void MarkAsUpdated() => lastUpdated = DateTime.UtcNow.ToString("o");

        public LevelTracker GetLevelData(string id) => levelProgress.Find(x => x.levelID == id);
    }
}