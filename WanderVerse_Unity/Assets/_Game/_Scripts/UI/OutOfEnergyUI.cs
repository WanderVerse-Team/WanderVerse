using UnityEngine;
using TMPro;
using UnityEngine.UI;
using WanderVerse.Backend.Services;

public class OutOfEnergyUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public Button closeButton;

    private void Awake()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(ClosePopup);
        }
    }

    private void Update()
    {
        if (timerText != null && EnergyManager.Instance != null)
        {
            timerText.text = "Resets in: " + EnergyManager.Instance.GetTimeUntilMidnight();
        }
    }

    private void ClosePopup()
    {
        if (AudioManager.Instance != null) AudioManager.Instance.PlayClick();

        gameObject.SetActive(false);
    }
}