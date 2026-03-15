using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircularProgressUI : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage;
    public TextMeshProUGUI percentageText;

    [Range(0f, 1f)]
    public float progress = 0f;

    public void SetProgress(float value)
    {
        progress = Mathf.Clamp01(value);

        if (fillImage != null)
            fillImage.fillAmount = progress;

        if (percentageText != null)
            percentageText.text = Mathf.RoundToInt(progress * 100f) + "%";
    }
}
