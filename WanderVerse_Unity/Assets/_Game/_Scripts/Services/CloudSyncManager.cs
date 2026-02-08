using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using WanderVerse.Framework.Data;

namespace WanderVerse.Backend.Services
{
    public class CloudSyncManager : MonoBehaviour
    {
        public static CloudSyncManager Instance { get; private set; }
        
        // RAM Cache
        public PlayerData CurrentData { get; private set; }

        [Header("API Config")]
        [SerializeField] private string _productionURL = "https://server-backend-eight.vercel.app/api/sync";

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); }
        }

        public void InitializeData(string userId)
        {
            // TO DO: @Senmith - Uncomment the following line when LocalDataManager is ready and delete the "PlayerData local = null;" line below it
            // PlayerData local = LocalDataManager.Load(userId);
            PlayerData local = null; // Temporary fix to prevent errors

            if (local != null)
            {
                CurrentData = local;
                Debug.Log($"[Sync] Loaded Local Data from Disk. XP: {CurrentData.xp}");
            }
            else
            {
                CurrentData = new PlayerData { userID = userId };
                Debug.Log("[Sync] No Local Data found. Created New Profile.");
                
                // TO DO: @Senmith - Uncomment the below line
                // LocalDataManager.Save(CurrentData);
            }

            StartCoroutine(FetchCloudData(userId));
        }

        public int GetHighscoreForLevel(string levelID)
        {
            if (CurrentData == null) return 0;
            LevelTracker tracker = CurrentData.GetLevelData(levelID);
            return tracker != null ? tracker.highScore : 0;
        }

        public void SetHighscoreForLevel(string levelID, int currentRunScore, int stars)
        {
            if (CurrentData == null) return;

            LevelTracker tracker = CurrentData.GetLevelData(levelID);

            if (tracker != null)
            {
                if (currentRunScore > tracker.highScore)
                {
                    tracker.highScore = currentRunScore;
                }
                
                tracker.attempts++;

                if (stars > tracker.starsEarned)
                {
                    tracker.starsEarned = stars;
                }
            }
            else
            {
                LevelTracker newTracker = new LevelTracker
                {
                    levelID = levelID,
                    highScore = currentRunScore,
                    starsEarned = stars,
                    attempts = 1,
                    isUnlocked = true
                };
                CurrentData.levelProgress.Add(newTracker);
            }

            SyncProgress(CurrentData);
        }

        public void AddTotalXP(int xpToAdd)
        {
            if (CurrentData == null) return;

            CurrentData.xp += xpToAdd;
            Debug.Log($"[Sync] Added {xpToAdd} XP. Total Now: {CurrentData.xp}");

            SyncProgress(CurrentData);
        }

        public void SyncProgress(PlayerData dataToSave)
        {
            CurrentData = dataToSave; 
            
            // TO DO: @Senmith - Uncomment the below line
            // LocalDataManager.Save(CurrentData); 

            if (FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                StartCoroutine(UploadRoutine(JsonUtility.ToJson(CurrentData)));
            }
        }

        private IEnumerator UploadRoutine(string json)
        {
            var user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null) yield break;

            Task<string> tokenTask = user.TokenAsync(true);
            float timer = 0f;
            while (!tokenTask.IsCompleted) {
                timer += Time.deltaTime;
                if (timer > 10f) { Debug.LogError("Sync Timeout"); yield break; }
                yield return null;
            }
            
            if (tokenTask.IsCanceled || tokenTask.IsFaulted) yield break;

            using (UnityWebRequest request = new UnityWebRequest(_productionURL, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                request.SetRequestHeader("Authorization", "Bearer " + tokenTask.Result);

                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[Cloud] Sync Success!");
                    
                    // TO DO: @Randiv - Uncomment this block when EnergyManager is ready
                    /*
                    string dateHeader = request.GetResponseHeader("Date");
                    if (!string.IsNullOrEmpty(dateHeader) && EnergyManager.Instance != null) 
                    {
                        DateTime serverTime = DateTime.Parse(dateHeader).ToUniversalTime();
                        EnergyManager.Instance.SyncTime(serverTime); 
                    }
                    */
                }
                else
                {
                    Debug.LogError($"[Cloud] Sync Failed: {request.error}");
                }
            }
        }
        
        private IEnumerator FetchCloudData(string userId) { yield return null; }
    }
}