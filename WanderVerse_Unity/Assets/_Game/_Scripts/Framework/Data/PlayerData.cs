using System;
using System.Collections.Generic;

namespace WanderVerse.Framework.Data 
{
    [Serializable]
    public class PlayerData
    {
        // Progress Data 
        public int xp;
        public int currentLevel;
        public List<int> levelStars; 

        // User Preferences (Onboarding) 
        public int selectedGrade;        
        public string selectedSubject;  
        public bool hasCompletedOnboarding; // FALSE = Show Grade/Subject selection; TRUE = Skip to Map

        // Metadata for Sync 
        public string lastUpdated; 


        public PlayerData()
        {
            // Initial Progress
            xp = 0;
            currentLevel = 1;
            levelStars = new List<int>();

            // Default Preferences for the Pilot
            selectedGrade = 3;
            selectedSubject = "Maths";
            hasCompletedOnboarding = false;

            lastUpdated = DateTime.UtcNow.ToString("o");
        }

        
        public void MarkAsUpdated()
        {
            lastUpdated = DateTime.UtcNow.ToString("o");
        }
    }
}