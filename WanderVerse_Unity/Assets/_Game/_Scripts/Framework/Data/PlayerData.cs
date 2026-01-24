using System;
using System.Collections.Generic;

[Serializable]
public class PlayerData
{
    public int xp;
    public int currentLevel;
    public List<int> levelStars; 
    public string lastUpdated;

    public PlayerData()
    {
        xp = 0;
        currentLevel = 1;
        levelStars = new List<int>();
        lastUpdated = DateTime.Now.ToString();
    }
}