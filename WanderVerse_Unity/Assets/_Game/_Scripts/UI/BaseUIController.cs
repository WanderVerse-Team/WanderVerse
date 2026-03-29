using UnityEngine;

public class BaseUIController : MonoBehaviour
{
    virtual protected void Awake()
    {
        if (Screen.orientation != ScreenOrientation.Portrait)
        {
            Screen.orientation = ScreenOrientation.Portrait;
        }
    }
}