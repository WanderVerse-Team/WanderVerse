using System;
using UnityEngine;
using WanderVerse.Framework.Data;

namespace WanderVerse.Backend.Services
{
    public class EnergyManager : MonoBehaviour
    {
        public static EnergyManager Instance { get; private set; }

        private EnergyBarUIController energyUI;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            CheckDailyReset();
            RefreshEnergyUI();
        }

        /// <summary>
        /// Compares the last saved date with today's date. 
        /// If it's a new day (past 12 AM), energy resets to max energy.
        /// </summary>
        public void CheckDailyReset()
        {
            PlayerData data = CloudSyncManager.Instance?.CurrentData;
            if (data == null) return;

            // Convert the Unix timestamp to the local device's Date (12:00 AM boundary)
            DateTime lastResetDate = DateTimeOffset.FromUnixTimeSeconds(data.lastDailyResetTimestamp).LocalDateTime.Date;
            DateTime currentDate = DateTime.Now.Date;

            if (currentDate > lastResetDate)
            {
                Debug.Log("[EnergyManager] A new day has started! Resetting energy to max.");
                data.energy = data.maxEnergy;
                data.lastDailyResetTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

                CloudSyncManager.Instance.SyncProgress(data);
                RefreshEnergyUI();
            }
            else if (currentDate < lastResetDate)
            {
                Debug.LogWarning("[EnergyManager] Device clock was moved backwards. Updating timestamp.");
                data.lastDailyResetTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                CloudSyncManager.Instance.SyncProgress(data);
            }
            else
            {
                RefreshEnergyUI();
            }
        }

        /// <summary>
        /// Deducts 1 energy bar when the player starts a level.
        /// </summary>
        public bool TryConsumeEnergy()
        {
            CheckDailyReset();

            PlayerData data = CloudSyncManager.Instance?.CurrentData;
            if (data == null) return false;

            if (data.energy > 0)
            {
                data.energy--;

                CloudSyncManager.Instance.SyncProgress(data);
                Debug.Log($"[EnergyManager] Consumed 1 Energy. Remaining: {data.energy}");

                RefreshEnergyUI();
                return true;
            }

            Debug.Log("[EnergyManager] 0 Energy! Come back tomorrow.");
            RefreshEnergyUI();
            return false;
        }

        /// <summary>
        /// Helper for Seshani's UI to display a "Reset in HH:MM" countdown.
        /// </summary>
        public string GetTimeUntilMidnight()
        {
            DateTime now = DateTime.Now;
            DateTime midnight = now.Date.AddDays(1);
            TimeSpan timeLeft = midnight - now;

            // Returns format "14:30" (Hours:Minutes)
            return string.Format("{0:D2}:{1:D2}", timeLeft.Hours, timeLeft.Minutes);
        }

        /// <summary>
        /// Called by CloudSyncManager when a successful server connection is made.
        /// Uses the true server time to verify if a daily reset should happen, 
        /// overriding any local device clock tampering.
        /// </summary>
        public void SyncTime(DateTime trueServerTime)
        {
            PlayerData data = CloudSyncManager.Instance?.CurrentData;
            if (data == null) return;

            DateTime lastResetDate = DateTimeOffset.FromUnixTimeSeconds(data.lastDailyResetTimestamp).LocalDateTime.Date;
            DateTime serverLocalDate = trueServerTime.ToLocalTime().Date;

            if (serverLocalDate > lastResetDate)
            {
                Debug.Log("[EnergyManager] Server verified a new day! Resetting energy.");
                data.energy = data.maxEnergy;
                data.lastDailyResetTimestamp = new DateTimeOffset(trueServerTime).ToUnixTimeSeconds();

                CloudSyncManager.Instance.SyncProgress(data);
                RefreshEnergyUI();
            }
            else if (serverLocalDate < lastResetDate)
            {
                Debug.LogWarning("[EnergyManager] Server detected future-tampering! Reverting timestamp to reality.");
                data.lastDailyResetTimestamp = new DateTimeOffset(trueServerTime).ToUnixTimeSeconds();

                CloudSyncManager.Instance.SyncProgress(data);
                RefreshEnergyUI();
            }
            else
            {
                Debug.Log("[EnergyManager] Server time synced. No reset needed yet.");
                RefreshEnergyUI();
            }
        }

        /// <summary>
        /// Refreshes the energy bar UI if it exists in the active scene.
        /// </summary>
        private void RefreshEnergyUI()
        {
            if (energyUI == null)
            {
                energyUI = FindObjectOfType<EnergyBarUIController>();
            }
            if (energyUI != null)
            {
                energyUI.RefreshEnergyUI();
            }
        }
    }
}