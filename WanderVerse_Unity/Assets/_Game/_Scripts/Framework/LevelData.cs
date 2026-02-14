using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "WanderVerse/Level Data", order = 1)]
public class LevelData : ScriptableObject
{
    // ========================================================================
    // 1. GLOBAL SETTINGS (Applies to all games)
    // ========================================================================
    [Header("--- 1. LESSON INFO ---")]
    public string lessonID;
    public string lessonTitle;

    [Header("--- 2. LEVEL INFO ---")]
    public string levelID;
    public string levelTitle;
    // Island?Map? -> enum
    [TextArea] public string description;
    public GameType gameType;          // Selects which logic to run

    [Header("--- 3. WIN CONDITIONS ---")]
    public int targetScore = 10;
    public int[] possibleTargets;

    [Tooltip("How many points to add for a correct answer?")]
    public int pointsForCorrect = 1;

    [Tooltip("How many points to deduct for a wrong answer?")]
    public int pointsForWrong = 1;

    [Tooltip("Set to 0 to allow infinite mistakes. Enter ONLY POSITIVE values")]
    public int maxMistakes = 0;

    [Header("--- 4. TIMER ---")]
    public bool useTimer = false;
    public float levelTimeLimit = 0f;

    // ========================================================================
    // 2. XP SYSTEM
    // ========================================================================
    [Header("--- 5. SCORING & XP ---")]

    [Tooltip("The MAXIMUM XP a player gets for a Perfect Run (0 Mistakes).")]
    public int maxXpReward = 100;

    [Tooltip("The MINIMUM. XP will never drop below this amount, no matter how many mistakes.")]
    public int baseXpReward = 20;

    [Tooltip("How much XP is lost for every single mistake.")]
    public int xpDeductionPerMistake = 5;

    [Tooltip("Bonus XP given for replaying a level perfectly (0 mistakes) when the maximum possible high score was achieved in a previous attempt.")]
    public int perfectRunBonus = 5;

    // ========================================================================
    // 3. STAR THRESHOLDS (Visuals Only)
    // ========================================================================
    [Header("--- 6. VISUAL STARS ---")]
    [Tooltip("XP does NOT depend on stars. This is just for the UI.")]
    public int maxMistakesFor3Stars = 0;
    public int maxMistakesFor2Stars = 2;

    [Header("--- 7. AUDIO & VISUALS ---")]
    //public Sprite backgroundImage;
    //public GameObject environmentPrefab; // Forest, Cave, Space
    public AudioClip backgroundMusic;
    //public AudioClip instructionAudio;   // "Find the Cone" 
    //[TextArea] public string instructionText;

    // ========================================================================
    // 4. DATA BUCKETS (Fill only the section matching your GameType)
    // ======================================================================== 

    [Header("--- MODE: COUNTING ---")]
    // For: Hungry Golem (Counting/Skip Counting)
    public List<GameObject> spawnItems;      // Fruits to spawn
    public List<GameObject> distractors;     // "Junk" fruit (e.g.- Bananas, Oranges)

    public int validValue = 1;            // The correct number to counts
    public float spawnRate = 2.0f;
    public float itemFallSpeed = 3.0f;

    //[Header("--- MODE: NUMBERS 1 ---")]
    // Add variables needed for Numbers 1 lesson

    //[Header("--- MODE: ADDITION 1 ---")]
    // Add variables needed for Addition 1 lesson

    //[Header("--- MODE: MEASURING LENGTH 1 ---")]
    // Add variables needed for Measuring Length 1 lesson

    //[Header("--- MODE: SUBTRACTION 1 ---")]
    // Add variables needed for Subtraction 1 lesson

    //[Header("--- MODE: TIME ---")]
    // Add variables needed for Time lesson

    //[Header("--- MODE: MULTIPLICATION 1 ---")]
    // Add variables needed for Multiplication 1 lesson

    //[Header("--- MODE: SOLID OBJECTS & SHAPES ---")]
    // Add variables needed for Solid Objects & Shapes lesson

    //[Header("--- MODE: DIVISION 1 ---")]
    // Add variables needed for Division 1 lesson

    //[Header("--- MODE: FRACTIONS ---")]
    // Add variables needed for Fractions lesson

    //[Header("--- MODE: DIRECTIONS ---")]
    // Add variables needed for Directions lesson

}

// ========================================================================
// 5. HELPER CLASSES (The Detailed Structures)
// ========================================================================

public enum GameType
{
    Spawner,
    Numbers1,
    Addition1,
    MeasuringLength1,
    Subtraction1,
    Time,
    Multiplication1,
    SolidObjectsAndShapes,
    Division1,
    Fractions,
    Directions
}
