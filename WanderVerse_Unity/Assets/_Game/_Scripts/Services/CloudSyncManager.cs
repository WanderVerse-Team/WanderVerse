using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore; 
using WanderVerse.Framework.Data;
using wanderVerse.Backend; 

namespace WanderVerse.Backend.Services
{
    // Helper class to read Senmith's JSON keys
    [Serializable]
    public class KeyResponse
    {
        public string status;
        public string key;
        public string iv;
    }

    public class CloudSyncManager : MonoBehaviour
    {
        public static CloudSyncManager Instance { get; private set; }
        
        // RAM Cache
        public PlayerData CurrentData { get; private set; }
        public bool IsGuest { get; private set; } = false;

        [Header("API Config")]
        [SerializeField] private string _keyURL = "https://server-backend-eight.vercel.app/api/keys";
        
        private string _leaderboardURL = "https://server-backend-eight.vercel.app/api/leaderboard";

        private readonly string _guestKey = "12345678901234567890123456789012"; 
        private readonly string _guestIV  = "1234567890123456";

        private FirebaseFirestore _db;
        private bool _isSaving = false;

        private void Awake()
        {
            if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
            else { Destroy(gameObject); }
        }

        private void Start()
        {
            _db = FirebaseFirestore.DefaultInstance;
        }

        
        public void InitializeAsGuest()
        {
            Debug.Log("[Sync] Initializing Guest Mode...");
            IsGuest = true;

            if (LocalDataManager.Instance != null)
            {
                LocalDataManager.Instance.InitializeSecurity(_guestKey, _guestIV);
            }

            // 2. Load Local Data
            // TO DO: @Senmith - Uncomment the following line when LocalDataManager is ready
            // PlayerData local = LocalDataManager.Instance.LoadGame();
            PlayerData local = null; // Temporary fix to prevent errors

            if (local != null)
            {
                CurrentData = local;
                Debug.Log($"[Sync] Guest Data Loaded. XP: {CurrentData.xp}");
            }
            else
            {
                CurrentData = new PlayerData { userID = "guest_" + System.Guid.NewGuid().ToString() };
                
                // TO DO: @Senmith - Uncomment the below line
                // LocalDataManager.Instance.SaveGame(CurrentData);
                
                Debug.Log("[Sync] New Guest Profile Created.");
            }
        }

        public void InitializeAsUser(string userId)
        {
            Debug.Log($"[Sync] Initializing User Mode for {userId}...");
            IsGuest = false;
            StartCoroutine(InitializeUserRoutine(userId));
        }

        private IEnumerator InitializeUserRoutine(string userId)
        {
            yield return StartCoroutine(FetchKeysFromVercel());

            // TO DO: @Senmith - Uncomment the below line when LocalDataManager is ready and delete the "PlayerData local = null;" line
            // PlayerData local = LocalDataManager.Instance.LoadGame();
            PlayerData local = null; // Temporary fix

            if (local != null)
            {
                CurrentData = local;
                Debug.Log($"[Sync] Local Data Loaded (XP: {CurrentData.xp}). Checking Cloud...");
            }

            yield return StartCoroutine(FetchCloudData(userId));
            
            if (CurrentData != null && CurrentData.userID != userId)
            {
                Debug.Log("[Sync] Cloud was empty or inferior. Uploading Local Data to Cloud.");
                CurrentData.userID = userId; 
                SyncProgress(CurrentData);  
            }
        }

        private IEnumerator FetchKeysFromVercel()
        {
            var user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null) yield break;

            Task<string> tokenTask = user.TokenAsync(true);
            yield return new WaitUntil(() => tokenTask.IsCompleted);

            if (tokenTask.Exception != null) 
            {
                Debug.LogError("[Sync] Failed to get Auth Token for Keys.");
                yield break;
            }

            using (UnityWebRequest request = UnityWebRequest.Get(_keyURL))
            {
                request.SetRequestHeader("Authorization", "Bearer " + tokenTask.Result);
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string json = request.downloadHandler.text;
                    KeyResponse response = JsonUtility.FromJson<KeyResponse>(json);

                    if (LocalDataManager.Instance != null && response != null)
                    {
                        LocalDataManager.Instance.InitializeSecurity(response.key, response.iv);
                        Debug.Log("[Sync] Keys fetched from Vercel. Security Initialized.");
                    }
                }
                else
                {
                    Debug.LogError($"[Sync] Failed to fetch keys: {request.error}");
                }
            }
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

        // SAVE GRADE & SUBJECT (ONBOARDING)
        // Seshani can call this function when her UI is ready.
        public void UpdateUserPreferences(int grade, string subject)
        {
            if (CurrentData == null) 
            {
                CurrentData = new PlayerData();
                if (!IsGuest && FirebaseAuth.DefaultInstance.CurrentUser != null)
                {
                    CurrentData.userID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
                }
                else if (IsGuest && string.IsNullOrEmpty(CurrentData.userID))
                {
                    CurrentData.userID = "guest_" + System.Guid.NewGuid().ToString();
                }
            }

            CurrentData.selectedGrade = grade;
            CurrentData.selectedSubject = subject;
            CurrentData.hasCompletedOnboarding = true;

            Debug.Log($"[Preferences] Saved: Grade {grade}, Subject {subject}");

            SyncProgress(CurrentData);
        }

        public void SyncProgress(PlayerData dataToSave)
        {
            CurrentData = dataToSave; 
            
            // TO DO: @Senmith - Uncomment the below line
            // LocalDataManager.Instance.SaveGame(CurrentData); 

            if (!IsGuest && FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                StartCoroutine(SaveToFirestoreRoutine(CurrentData));
            }
        }

        private IEnumerator SaveToFirestoreRoutine(PlayerData data)
        {
            if (_isSaving) yield break;
            _isSaving = true;

            var dataMap = new Dictionary<string, object>
            {
                { "xp", data.xp },
                { "json", JsonUtility.ToJson(data) },
                { "lastUpdated", FieldValue.ServerTimestamp }
            };

            var task = _db.Collection("users").Document(data.userID).SetAsync(dataMap, SetOptions.MergeAll);            
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                Debug.LogWarning($"[Cloud] Save Pending (Offline) or Failed: {task.Exception.InnerException?.Message}");
            }
            else
            {
                Debug.Log("[Cloud] Sync Success!");

                // TO DO: @Randiv - Uncomment this block when EnergyManager is ready
                /*
                // Note: Firestore does not return a Date header like HTTP. 
                // You can use System time or fetch ServerTimestamp separately.
                if (EnergyManager.Instance != null) 
                {
                    // EnergyManager.Instance.SyncTime(DateTime.UtcNow); 
                }
                */
            }

            _isSaving = false;
        }
        
        private IEnumerator FetchCloudData(string userId) 
        {
            Debug.Log("[Cloud] Fetching profile...");
            var task = _db.Collection("users").Document(userId).GetSnapshotAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Result.Exists)
            {
                string json = task.Result.GetValue<string>("json");
                
                PlayerData cloudData = JsonUtility.FromJson<PlayerData>(json);
                
                if (CurrentData == null || cloudData.xp > CurrentData.xp)
                {
                    CurrentData = cloudData;
                    Debug.Log($"[Cloud] Profile Loaded! XP: {CurrentData.xp}");
                    
                    // TO DO: @Senmith - Update local disk cache with new cloud data
                    // LocalDataManager.Instance.SaveGame(CurrentData);
                }
            }
            else
            {
                // New User logic
                if (CurrentData == null) 
                {
                    CurrentData = new PlayerData { userID = userId };
                    Debug.Log("[Cloud] New User. Created New Profile.");
                    
                    // TO DO: @Senmith - Uncomment the below line
                    // LocalDataManager.Instance.SaveGame(CurrentData);
                    
                    SyncProgress(CurrentData);
                }
            }
        }

        // To get the leaderboard (for senmith's leaderboardController)
        public IEnumerator FetchLeaderboard(System.Action<string> onSuccess, System.Action<string> onFailure)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(_leaderboardURL))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[Leaderboard] Success: " + request.downloadHandler.text);
                    onSuccess?.Invoke(request.downloadHandler.text);
                }
                else
                {
                    Debug.LogError("[Leaderboard] Failed: " + request.error);
                    onFailure?.Invoke(request.error);
                }
            }
        }
    }
}