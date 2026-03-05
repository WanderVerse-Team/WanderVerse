using UnityEngine;

public class GreenSwitch : MonoBehaviour
{
    [Header("--- Reference ---")]
    public FractionsLevelController levelController;

    // Attach this to your Green Button's OnClick() in the Inspector
    public void OnButtonPressed()
    {
        Debug.Log("[GreenSwitch] Pressed! Next item coming...");

        if (levelController != null)
            levelController.OnGreenSwitchPressed();
        else
            Debug.LogError("[GreenSwitch] No LevelController assigned!");
    }
}