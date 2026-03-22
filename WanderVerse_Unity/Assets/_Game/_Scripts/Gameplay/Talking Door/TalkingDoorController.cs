using UnityEngine;
using UnityEngine.UI;
using TMPro; 
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

    private bool firstAudioPlayed = false;

    protected override GameType SupportedGameType => GameType.Numbers1;

    protected override void InitializeLevel()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        questionDeck = new List<QuestionItem>(levelData.questions);
        ShuffleList(questionDeck);
        GenerateNextQuestion();
    }

    public override void Update()
    {
        base.Update();

        if (isGameActive && !firstAudioPlayed)
        {
            firstAudioPlayed = true;
            PlayCurrentPromptAudio();
        }
    }


    private void GenerateNextQuestion()
    {
        if (currentScore >= levelData.targetScore || questionDeck.Count == 0)
        {
            EndLevel();
            return;
        }

        currentQuestion = questionDeck[0];
        questionDeck.RemoveAt(0);

        doorPromptText.text = currentQuestion.promptText;
        
        // if (currentQuestion.promptAudio != null && doorSpeaker != null)
        // {
        //     doorSpeaker.PlayOneShot(currentQuestion.promptAudio);
        // }

        if (isGameActive)
        {
            PlayCurrentPromptAudio();
        }

        List<string> options = new List<string>();
        options.Add(currentQuestion.correctString);

        if (currentQuestion.wrongAnswers.Count >= 2)
        {
            List<string> shuffledWrong = new List<string>(currentQuestion.wrongAnswers);
            ShuffleList(shuffledWrong);
            options.Add(shuffledWrong[0]);
            options.Add(shuffledWrong[1]);
        }
        else
        {
            Debug.LogError($"Question '{currentQuestion.correctString}' needs at least 2 wrong answers!");
            options.Add("X");
            options.Add("Y");
        }

        ShuffleList(options);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (i < options.Count)
            {
                answerButtons[i].gameObject.SetActive(true);
                buttonLabels[i].text = options[i];
                answerButtons[i].interactable = true; 
                answerButtons[i].image.color = Color.white;

                string selectedAnswer = options[i];
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(selectedAnswer, i));
            }
            else
            {
                answerButtons[i].gameObject.SetActive(false);
            }
        }
        if (doorPromptText != null)
            {
                SinhalaTextFixer fixer = doorPromptText.GetComponent<SinhalaTextFixer>();
                
                if (fixer != null)
                {
                    fixer.SetFixedText(currentQuestion.promptText);
                }
                else
                {
                    doorPromptText.text = currentQuestion.promptText;
                }
            }
    }

    public void OnAnswerSelected(string answer, int buttonIndex)
    {
        if (answer == currentQuestion.correctString)
        {
            HandleCorrectAnswer();
            
            if (feedbackSpeaker != null) feedbackSpeaker.PlayOneShot(correctSound);
            StartCoroutine(WaitAndNext());
        }
        else
        {
            HandleWrongAnswer();
            if (feedbackSpeaker != null) feedbackSpeaker.PlayOneShot(wrongSound);
            answerButtons[buttonIndex].interactable = false; 
        }
    }

    private IEnumerator WaitAndNext()
    {
        yield return new WaitForSeconds(1.0f);
        GenerateNextQuestion();
    }

    private void EndLevel()
    {
        if (feedbackSpeaker != null) feedbackSpeaker.PlayOneShot(victorySound);
        Debug.Log("Level Complete!");
        base.EndLevel(true);
    }

    private void PlayCurrentPromptAudio()
    {
        if (currentQuestion != null && currentQuestion.promptAudio != null && doorSpeaker != null)
        {
            doorSpeaker.PlayOneShot(currentQuestion.promptAudio);
        }
    }

    //Function to shuffle the question list
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