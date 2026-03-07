using UnityEngine;
using UnityEngine.SceneManagement;
using WanderVerse.Backend.Services;
using WanderVerse.Framework.Data;

public class OnboardingManager : MonoBehaviour
{
    // 1. Link this to your Grade 3 Button
    public void SelectGrade(int grade)
    {
        if (CloudSyncManager.Instance.CurrentData != null)
        {
            CloudSyncManager.Instance.CurrentData.selectedGrade = grade;
            // Move to the next selection screen
            SceneManager.LoadScene("Scene_SubjectSelection");
        }
    }

    // 2. Link this to your Mathematics Button
    public void SelectSubject(string subject)
    {
        if (CloudSyncManager.Instance.CurrentData != null)
        {
            CloudSyncManager.Instance.CurrentData.selectedSubject = subject;
            // Move to the next selection screen
            SceneManager.LoadScene("Scene_LanguageSelection");
        }
    }

    // 3. Link this to your Sinhala Button
    public void SelectLanguage(string language)
    {
        if (CloudSyncManager.Instance.CurrentData != null)
        {
            PlayerData data = CloudSyncManager.Instance.CurrentData;
            
            // Save the language (Make sure Himashi added this field!)
            data.selectedLanguage = language; 
            
            // IMPORTANT: Set this to true so they don't see these screens again
            data.hasCompletedOnboarding = true; 

            // Save to Local and Cloud immediately
            CloudSyncManager.Instance.SyncProgress(data);
            
            // Finally, go to the World Map
            SceneManager.LoadScene("Scene_WorldMap");
        }
    }
}