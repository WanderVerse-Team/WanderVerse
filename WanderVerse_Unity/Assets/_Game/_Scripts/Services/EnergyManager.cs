using System;
using UnityEngine;
using WanderVerse.Framework.Data;

namespace WanderVerse.Backend.Services
{
    public class EnergyManager : MonoBehaviour
    {
        public static EnergyManager Instance { get; private set; }

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
            // Check if we passed midnight as soon as the game loads
            CheckDailyReset();
        }

        /// <summary>
        /// Compares the last saved date with today's date. 
        /// If it's a new day (past 12 AM), energy resets to 6.
        /// </summary>
        public void CheckDailyReset()
        {
            PlayerData data = CloudSyncManager.Instance?.CurrentData;
            if (data == null) return;

            // Convert the Unix timestamp to the local device's Date (12:00 AM boundary)
            DateTime lastResetDate = DateTimeOffset.FromUnixTimeSeconds(data.lastDailyResetTimestamp).LocalDateTime.Date;
            DateTime currentDate = DateTimeOffset.Now.Date;

            // If the current date is strictly greater than the last saved date, midnight has passed!
            if (currentDate > lastResetDate)
            {
                Debug.Log("[EnergyManager] A new day has started! Resetting energy to 6.");
                data.energy = data.maxEnergy;
                data.lastDailyResetTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();

                CloudSyncManager.Instance.SyncProgress(data);
            }
            // If they set their clock backwards, do NOT update the timestamp!
            // Leave it in the future. They will not get any energy until real time catches up to tomorrow.
            else if (currentDate < lastResetDate)
            {
                Debug.LogWarning("[EnergyManager] Device clock was moved backwards. Updating timestamp.");
                // data.lastDailyResetTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                // CloudSyncManager.Instance.SyncProgress(data);
            }
        }

        /// <summary>
        /// Deducts 1 energy bar when the player starts a level.
        /// </summary>
        public bool TryConsumeEnergy()
        {
            // Always run the reset check first, just in case they left the app open over midnight
            CheckDailyReset();

            PlayerData data = CloudSyncManager.Instance.CurrentData;
            if (data == null) return false;

            if (data.energy > 0)
            {
                data.energy--;

                CloudSyncManager.Instance.SyncProgress(data);
                Debug.Log($"[EnergyManager] Consumed 1 Energy. Remaining: {data.energy}");
                return true;
            }

            Debug.Log("[EnergyManager] 0 Energy! Come back tomorrow.");
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

            // SCENARIO 1: A legitimate new day has passed!
            if (trueServerTime.Date > lastResetDate)
            {
                Debug.Log("[EnergyManager] Server verified a new day! Resetting energy.");
                data.energy = data.maxEnergy;
                data.lastDailyResetTimestamp = new DateTimeOffset(trueServerTime).ToUnixTimeSeconds();

                CloudSyncManager.Instance.SyncProgress(data);
            }
            // SCENARIO 2: The Server caught them returning from a fake future! (Your brilliant catch)
            else if (trueServerTime.Date < lastResetDate)
            {
                Debug.LogWarning("[EnergyManager] Server detected future-tampering! Reverting timestamp to reality.");
                // We do NOT give them energy here. We just fix their broken save file.
                data.lastDailyResetTimestamp = new DateTimeOffset(trueServerTime).ToUnixTimeSeconds();

                CloudSyncManager.Instance.SyncProgress(data);
            }
            // SCENARIO 3: Same day. Nothing to do.
            else
            {
                Debug.Log("[EnergyManager] Server time synced. No reset needed yet.");
            }
        }
    }
}