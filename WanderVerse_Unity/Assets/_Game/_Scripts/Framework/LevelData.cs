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

    [Tooltip("Enter ONLY POSITIVE values")]
    public int pointsForCorrect = 1;

    [Tooltip("Enter ONLY POSITIVE values")]
    public int pointsForWrong = 1;

    public bool useTimer = false;
    public float levelTimeLimit = 0f;

    [Tooltip("Set to 0 to allow infinite mistakes. Enter ONLY POSITIVE values")]
    public int maxMistakes = 3;

    //[Header("--- 4. AUDIO & VISUALS ---")]
    //public Sprite backgroundImage;
    //public GameObject environmentPrefab; // Forest, Cave, Space
    //public AudioClip backgroundMusic;
    //public AudioClip instructionAudio;   // "Find the Cone" 
    //[TextArea] public string instructionText;

    // ========================================================================
    // 4. DATA BUCKETS (Fill only the section matching your GameType)
    // ========================================================================

    // [Header("--- MODE: RECOGNITION & QUIZ ---")]
    //// For: Talking Door (Numbers), Shape Town (Names)
    // public List<QuestionItem> questions; 

    [Header("--- MODE: SPAWNER / COUNTING ---")]
    // For: Hungry Golem (Counting/Skip Counting)
    public List<GameObject> spawnItems;      // Fruits to spawn
    public List<GameObject> distractors;     // "Junk" fruit (e.g.- Bananas, Oranges)
    public int countByStep = 1;              // 1, 2, 5, or -1 (Backwards)
    public float spawnRate = 2.0f;
    public float itemFallSpeed = 3.0f;

    //[Header("--- MODE: MATH OPERATIONS (GRID) ---")]
    //// For: Addition, Shelf (Sub), Candy (Mult), Cargo (Div)
    //public int mathTargetValue;              // The total power/weight needed
    //public int gridColumns = 2;              // Tens and Ones
    //public List<int> availableNumbers;       // Batteries/Cans available to drag
    //public int multiplierValue;              // For Candy Factory
    //public int divisorCount;                 // For Cargo (Trucks)

    //[Header("--- MODE: MEASUREMENT ---")]
    //// For: Ruins & Rulers
    //public List<MeasurementTool> tools;      // Pencil, Matchstick, Hand
    //public float objectTrueLength;           // Length in meters
    //public List<LogicQuestion> logicQuestions; // "Is it > 2m?"

    //[Header("--- MODE: SEQUENCE & SORTING ---")]
    //// For: Stepping Stones, Potion Sorter, Days/Months
    //public bool isAscending = true;          // Ascending vs Descending
    //public List<string> correctSequence;     // "Mon, Tue, Wed" or "8, 15, 25"
    //public List<GameObject> sortableObjects; // Potions, Bottles, Calendar Tiles

    //[Header("--- MODE: SHAPES & BUILDING ---")]
    //// For: Shape Town Builder
    //public GameObject targetStructure;       // The Rocket/House to build
    //public List<ShapePart> requiredParts;    // Cylinder, Cone, etc.

    //[Header("--- MODE: FRACTIONS ---")]
    //// For: Bakery Builder
    //public FractionType fractionMode;        // Halves, Quarters 
    //public bool allowSlicing = true;         // Drag to slice vs Drag to combine 

    //[Header("--- MODE: TIME & CLOCK ---")]
    //// For: Taxi Time Traveler
    //public int targetHour;                   // Set clock to 7 
    //public int targetMinute;                 // Set clock to 00
    //public List<string> calendarEvents;      // "Independence Day" -> "Feb"

    //[Header("--- MODE: DIRECTIONS (WORM) ---")]
    //// For: Worm Eating Apple
    //public int gridSize = 5;                 // 5x5 Grid
    //public bool useCompass = false;          // Left/Right vs North/South 
    //public List<Vector2> appleLocations;     // Grid coordinates for apples
    //public List<DirectionStep> requiredPath; // "Go North, then East" 

    //[Header("--- NARRATIVE ---")]
    //// For: Talking Door Intro, Time Chief, Golem waking up
    //public bool hasOpeningDialogue;
    //public List<DialogueLine> dialogueLines;
}

// ========================================================================
// 5. HELPER CLASSES (The Detailed Structures)
// ========================================================================

public enum GameType
{
    //TalkingDoor,        // L1 Numbers
    //PlaceValue,         // L2 Treasure
    //ComparingBridge,    // L3 Bridge
    //SequenceStones,     // L4 Stones
    //OrderingPotions,    // L5 Potions
    Spawner,              // Counting/Spawners
    //MathGrid,           // Add/Sub/Mult/Div
    //Measurement,        // Rulers
    //TimeTaxi,           // Clock/Calendar
    //ShapeBuilder,       // 3D Solids
    //BakeryFractions,    // Fractions
    //WormDirections      // Directions
}

//[System.Serializable]
//public class QuestionItem 
//{
//    [Tooltip("Text displayed or spoken")]
//    public string promptText;           // "Fourteen!"
//    public AudioClip promptAudio;       // Audio of "Fourteen"
//    public Sprite visualClue;           // Image of 5 apples

//    [Tooltip("The correct answer")]
//    public string correctString;        // "14"
//    public int correctInt;              // For math checking

//    [Tooltip("Wrong options for the player to avoid")]
//    public List<string> wrongAnswers;   // "21", "40"
//}

//[System.Serializable]
//public class MeasurementTool
//{
//    public string toolName;             // "Matchstick"
//    public int countResult;             // 10 times
//}

//[System.Serializable]
//public class LogicQuestion
//{
//    public string questionText;         // "How wide is the gap?"
//    public List<string> options;        // "3 meters", "> 2 meters"
//    public int correctOptionIndex;      
//}

//[System.Serializable]
//public class ShapePart
//{
//    public string partName;             // "Nose"
//    public GameObject shapePrefab;      // Cone
//    public Vector3 requiredPosition;    // Where it snaps to
//}

//[System.Serializable]
//public class DirectionStep
//{
//    public string instruction;          // "Go North" 
//    public Vector2 direction;           // (0, 1)
//}

//[System.Serializable]
//public class DialogueLine
//{
//    public string speakerName;          // "Time Chief"
//    [TextArea] public string text;      // "We need to travel to the day before Wednesday!"
//    public AudioClip voiceOver;
//}

//public enum FractionType { Halves, Quarters, Combined }
