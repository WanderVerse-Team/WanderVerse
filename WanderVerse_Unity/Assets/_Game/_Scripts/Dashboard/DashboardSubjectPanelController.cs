using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WanderVerse.Backend.Services;
using WanderVerse.Framework.Data;

public class DashboardSubjectPanelController : MonoBehaviour
{
    [Header("Dropdown")]
    public TMP_Dropdown subjectDropdown;

    [Header("Subject Info UI")]
    public TextMeshProUGUI subjectNameText;
    public Slider subjectProgressBar;
    public TextMeshProUGUI subjectProgressPercentageText;

    [Header("Map Progress")]
    public TextMeshProUGUI map1ProgressText;
    public TextMeshProUGUI map2ProgressText;
    public TextMeshProUGUI map3ProgressText;

    [Header("Badges")]
    public GameObject[] badgeIcons;

    private void Start()
    {
        SetupDropdown();
        UpdateSubjectPanel();
    }

    private void SetupDropdown()
    {
        if (subjectDropdown == null) return;

        subjectDropdown.onValueChanged.RemoveAllListeners();
        subjectDropdown.onValueChanged.AddListener(delegate {
            OnSubjectChanged(subjectDropdown.value);
        });

        // Default to Maths
        subjectDropdown.value = 0;
        subjectDropdown.RefreshShownValue();
    }

    public void OnSubjectChanged(int index)
    {
        UpdateSubjectPanel();
    }

    public void UpdateSubjectPanel()
    {
        PlayerData data = CloudSyncManager.Instance.CurrentData;
        if (data == null) return;

        string selectedSubject = subjectDropdown != null
            ? subjectDropdown.options[subjectDropdown.value].text
            : "Maths";

        if (subjectNameText != null)
            subjectNameText.text = selectedSubject;

        float progress = CalculateSubjectProgress(data, selectedSubject);

        if (subjectProgressBar != null)
            subjectProgressBar.value = progress;

        if (subjectProgressPercentageText != null)
            subjectProgressPercentageText.text = $"{(progress * 100):F0}%";

        // Example map progress placeholders for now
        if (map1ProgressText != null)
            map1ProgressText.text = $"Island 1: {(progress * 100):F0}%";

        if (map2ProgressText != null)
            map2ProgressText.text = "Island 2: Locked";

        if (map3ProgressText != null)
            map3ProgressText.text = "Island 3: Locked";

        UpdateBadges(data, selectedSubject);
    }

    private float CalculateSubjectProgress(PlayerData data, string subject)
    {
        if (data.levelProgress == null || data.levelProgress.Count == 0)
            return 0f;

        int completed = 0;
        int total = data.levelProgress.Count;

        foreach (var level in data.levelProgress)
        {
            if (level.starsEarned > 0)
                completed++;
        }

        if (total == 0) return 0f;
        return (float)completed / total;
    }

    private void UpdateBadges(PlayerData data, string subject)
    {
        if (badgeIcons == null || badgeIcons.Length == 0) return;

        if (badgeIcons.Length > 0 && badgeIcons[0] != null)
            badgeIcons[0].SetActive(data.xp > 500);

        if (badgeIcons.Length > 1 && badgeIcons[1] != null)
            badgeIcons[1].SetActive(data.currentLevel > 5);

        if (badgeIcons.Length > 2 && badgeIcons[2] != null)
            badgeIcons[2].SetActive(false);
    }
}
