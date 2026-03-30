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

        var data = CloudSyncManager.Instance.CurrentData;

        int index = subjectDropdown.options.FindIndex(
            option => option.text == data.selectedSubject
        );

        if (index >= 0)
            subjectDropdown.value = index;

        subjectDropdown.onValueChanged.RemoveAllListeners();
        subjectDropdown.onValueChanged.AddListener(OnSubjectChanged);
    }

    public void OnSubjectChanged(int index)
    {
        string selectedSubject = subjectDropdown.options[index].text;

        if (CloudSyncManager.Instance != null)
        {
            var data = CloudSyncManager.Instance.CurrentData;

            CloudSyncManager.Instance.UpdateUserPreferences(
                data.selectedGrade,
                selectedSubject
            );
        }

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

        float map1 = CalculateMapProgress(data, "Island1");
        float map2 = CalculateMapProgress(data, "Island2");
        float map3 = CalculateMapProgress(data, "Island3");

        float progress = (map1 + map2 + map3) / 3f;

        if (subjectProgressBar != null)
            subjectProgressBar.value = progress;

        if (subjectProgressPercentageText != null)
            subjectProgressPercentageText.text = $"{(progress * 100):F0}%";

        // Example map progress placeholders for now
        if (map1ProgressText != null)
            map1ProgressText.text = $"Island 1: {(map1 * 100):F0}%";

        if (map2ProgressText != null)
            map2ProgressText.text = map1 >= 1f 
                ? $"Island 2: {(map2 * 100):F0}%"
                : "Island 2: Locked";

        if (map3ProgressText != null)
            map3ProgressText.text = map2 >= 1f 
                ? $"Island 3: {(map3 * 100):F0}%"
                : "Island 3: Locked";

        UpdateBadges(data, selectedSubject);
    }

    private float CalculateMapProgress(PlayerData data, string mapPrefix) 
    {
        if (data.levelProgress == null || data.levelProgress.Count == 0)
            return 0f;

        int completed = 0;
        int total = 0;

        foreach (var level in data.levelProgress)
        {
            if (!level.levelID.StartsWith(mapPrefix))
            continue;

            total++;

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
