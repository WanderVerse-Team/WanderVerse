using UnityEngine;
using UnityEngine.UI;
using WanderVerse.Backend.Services;

public class LevelSelectButton : MonoBehaviour
{
    [Header("Level Scene")]
    [Tooltip("Enter the exact name of the Unity Scene to load")]
    public string sceneToLoad = "";

    [Header("Energy Feedback")]
    [Tooltip("Drag 'Out of Energy' UI panel here")]
    public GameObject outOfEnergyPopup;

    // Link this method to the Button's OnClick() event in the Inspector
    public void OnClickPlayLevel()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("[LevelSelectButton] No GameManager found! Is the prefab missing from the Resources folder?");
            return;
        }
        if (EnergyManager.Instance == null)
        {
            Debug.LogError("[LevelSelectButton] No EnergyManager found! Is the prefab missing from the Resources folder?");
            return;
        }

        bool canPlay = EnergyManager.Instance.TryConsumeEnergy();

        if (canPlay)
        {
            Debug.Log($"[LevelSelectButton] Energy consumed! Telling GameManager to load: {sceneToLoad}");

            GameManager.Instance.LoadLevel(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("[LevelSelectButton] Not enough energy. Showing popup.");

            if (outOfEnergyPopup != null)
            {
                outOfEnergyPopup.SetActive(true);
            }
        }
    }
}