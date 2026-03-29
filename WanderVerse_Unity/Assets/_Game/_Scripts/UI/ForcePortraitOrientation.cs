using UnityEngine;

public class ForcePortraitOrientation : MonoBehaviour
{
    virtual protected void Awake()
    {
        if (Screen.orientation != ScreenOrientation.Portrait)
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
    }
}