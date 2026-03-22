using UnityEngine;
using UnityEngine.UI;
using WanderVerse.Backend.Services;
using WanderVerse.Framework.Data;

public class EnergyBarUIController : MonoBehaviour
{
    [Header("UI Reference")]
    public Image energyBarImage;

    [Header("Sprites by Energy Amount")]
    public Sprite energy0Sprite;
    public Sprite energy1Sprite;
    public Sprite energy2Sprite;
    public Sprite energy3Sprite;
    public Sprite energy4Sprite;
    public Sprite energy5Sprite;
    public Sprite energy6Sprite;

    private void OnEnable()
    {
        RefreshEnergyUI();
    }

    public void RefreshEnergyUI()
    {
        if (energyBarImage == null)
            return;

        if (CloudSyncManager.Instance == null || CloudSyncManager.Instance.CurrentData == null)
            return;

        PlayerData data = CloudSyncManager.Instance.CurrentData;

        int currentEnergy = Mathf.Clamp(data.energy, 0, 6);

        switch (currentEnergy)
        {
            case 0:
                energyBarImage.sprite = energy0Sprite;
                break;
            case 1:
                energyBarImage.sprite = energy1Sprite;
                break;
            case 2:
                energyBarImage.sprite = energy2Sprite;
                break;
            case 3:
                energyBarImage.sprite = energy3Sprite;
                break;
            case 4:
                energyBarImage.sprite = energy4Sprite;
                break;
            case 5:
                energyBarImage.sprite = energy5Sprite;
                break;
            case 6:
                energyBarImage.sprite = energy6Sprite;
                break;
        }
    }
}