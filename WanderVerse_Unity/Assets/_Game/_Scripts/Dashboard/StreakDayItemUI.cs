using UnityEngine;
using TMPro;

public class StreakDayItemUI : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dayText;
    public GameObject completedIcon;
    public GameObject todayHighlight;

    public void Setup(string label, bool isCompleted, bool isToday)
    {
        if (dayText != null)
            dayText.text = label;

        if (completedIcon != null)
            completedIcon.SetActive(isCompleted);

        if (todayHighlight != null)
            todayHighlight.SetActive(isToday);
    }
}