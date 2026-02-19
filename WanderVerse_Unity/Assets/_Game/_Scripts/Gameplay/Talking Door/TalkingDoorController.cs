using UnityEngine;
using UnityEngine.UI;
using TMPro; // Needed for TextMeshPro
using System.Collections.Generic;
using System.Collections;

public class TalkingDoorController : BaseLevelController
{
    [Header("--- Talking Door UI ---")]
    public TextMeshProUGUI doorPromptText;   // The text on the Door (e.g., "Find the number...")
    public Button[] answerButtons;           // Drag your 3 Buttons here
    public TextMeshProUGUI[] buttonLabels;   // Drag the Text inside those 3 buttons here

    [Header("--- Audio ---")]
    public AudioSource feedbackSpeaker;       // Source for correct/wrong sounds
    public AudioSource doorSpeaker;          // Source for the voice lines
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip victorySound;

    // Internal State
    private List<QuestionItem> questionDeck = new List<QuestionItem>();
    private QuestionItem currentQuestion;

    // 1. Tell the framework this is a "Numbers1" game (or whatever you named it)
    protected override GameType SupportedGameType => GameType.Numbers1;

    protected override void InitializeLevel()
    {
        // A. Load the "Deck"
        // We copy the list so we can remove items without breaking the original LevelData
        questionDeck = new List<QuestionItem>(levelData.questions);

        // B. Shuffle the Deck (Randomize order)
        ShuffleList(questionDeck);

        // C. Start the first question
        GenerateNextQuestion();
    }

    private void GenerateNextQuestion()
    {
        // 1. Check if we are done
        // We stop if we hit the target score OR if we run out of unique questions
        if (currentScore >= levelData.targetScore || questionDeck.Count == 0)
        {
            EndLevel();
            return;
        }

        // 2. Draw the top card
        currentQuestion = questionDeck[0];
        questionDeck.RemoveAt(0); // Remove it so it doesn't repeat!

        // 3. Setup the Door (Visuals/Audio)
         doorPromptText.text = currentQuestion.promptText; // e.g., "Five"
        
        if (currentQuestion.promptAudio != null && doorSpeaker != null)
        {
            doorSpeaker.PlayOneShot(currentQuestion.promptAudio);
        }

        // 4. PREPARE ANSWERS
        List<string> options = new List<string>();

        // A. Add the Correct Answer
        options.Add(currentQuestion.correctString);

        // B. Pick 2 Random Wrong Answers from the list
        if (currentQuestion.wrongAnswers.Count >= 2)
        {
            // Shuffle the wrong answers list first to get random ones
            List<string> shuffledWrong = new List<string>(currentQuestion.wrongAnswers);
            ShuffleList(shuffledWrong);

            // Take the first 2
            options.Add(shuffledWrong[0]);
            options.Add(shuffledWrong[1]);
        }
        else
        {
            // Fallback if you forgot to add enough wrong answers in Inspector
            Debug.LogError($"Question '{currentQuestion.correctString}' needs at least 2 wrong answers!");
            options.Add("X");
            options.Add("Y");
        }

        // C. Shuffle the Options (So the correct answer isn't always Button 1)
        ShuffleList(options);

        // 5. ASSIGN TO BUTTONS
        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < options.Count)
            {
                answerButtons[i].gameObject.SetActive(true);
                buttonLabels[i].text = options[i];
                
                // Reset button color (in case it was red from a previous mistake)
                answerButtons[i].interactable = true; 
                answerButtons[i].image.color = Color.white;

                // Remove old clicks and add new listener
                string selectedAnswer = options[i]; // Local copy for the closure
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(selectedAnswer, i));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false); // Hide unused buttons
            }
        }
        if (doorPromptText != null)
            {
                // Try to get the fixer component from the text object
                SinhalaTextFixer fixer = doorPromptText.GetComponent<SinhalaTextFixer>();
                
                if (fixer != null)
                {
                    // Use the fixer to display the Sinhala word correctly
                    fixer.SetFixedText(currentQuestion.promptText);
                }
                else
                {
                    // Fallback if no fixer is attached
                    doorPromptText.text = currentQuestion.promptText;
                }
            }
    }

    public void OnAnswerSelected(string answer, int buttonIndex)
    {
        if (answer == currentQuestion.correctString)
        {
            // --- CORRECT ---
            HandleCorrectAnswer(); // Base function updates score
            
            if (feedbackSpeaker != null) feedbackSpeaker.PlayOneShot(correctSound);

            // Optional: Make button green
            //answerButtons[buttonIndex].image.color = Color.green;

            // Wait a moment, then next question
            StartCoroutine(WaitAndNext());
        }
        else
        {
            // --- WRONG ---
            HandleWrongAnswer(); // Base function tracks mistakes

            if (feedbackSpeaker != null) feedbackSpeaker.PlayOneShot(wrongSound);

            // Visual Feedback: Make button red and disable it
            //answerButtons[buttonIndex].image.color = Color.red;
            answerButtons[buttonIndex].interactable = false; 
        }
    }

    private IEnumerator WaitAndNext()
    {
        yield return new WaitForSeconds(1.0f); // Small pause to celebrate
        GenerateNextQuestion();
    }

    private void EndLevel()
    {
        if (feedbackSpeaker != null) feedbackSpeaker.PlayOneShot(victorySound);
        Debug.Log("Level Complete!");
        
        // Call the Game Manager to save XP/Stars
        // (Assuming you have a CheckWinCondition or similar in BaseLevelController)
        // CheckWinCondition(); 
    }

    // Helper: Fisher-Yates Shuffle Algorithm
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}