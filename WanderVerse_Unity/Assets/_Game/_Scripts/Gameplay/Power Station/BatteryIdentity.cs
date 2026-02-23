using UnityEngine;
using TMPro;

/// <summary>
/// Attached to each battery prefab. Holds the digit value and updates the label.
/// </summary>
public class BatteryIdentity : MonoBehaviour
{
    [Header("--- Battery Data ---")]
    public int digitValue;               // 0-9, displayed on the battery

    [Header("--- Visuals ---")]
    public TextMeshProUGUI labelText;    // The TMP text on the battery showing the digit

    /// <summary>
    /// Called by PowerStationController when spawning batteries at runtime.
    /// </summary>
    public void Setup(int value)
    {
        digitValue = value;
        if (labelText != null)
            labelText.text = value.ToString();
    }
}
