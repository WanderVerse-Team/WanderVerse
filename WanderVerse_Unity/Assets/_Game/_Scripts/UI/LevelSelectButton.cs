using UnityEngine;

public class LevelSelectButton : MonoBehaviour
{
    [Tooltip("Drag the specific LevelData file here")]
    public LevelData levelDataForThisButton;

    [Tooltip("Enter the exact name of the Unity Scene to load")]
    public string sceneToLoad = "";

    // Link this method to the Button's OnClick() event in the Inspector
    public void OnClickPlayLevel() 
    {
        if (GameManager.Instance != null) 
        {
            GameManager.Instance.LoadLevel(levelDataForThisButton, sceneToLoad);
        }
        else 
        {
            Debug.LogError("No GameManager found in the scene! Make sure you start from the Boot Scene.");
        }
    }
}
