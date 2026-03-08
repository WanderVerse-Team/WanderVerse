using System.Diagnostics; // Added for Randiv's Editor tag
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore; 
using WanderVerse.Framework.Data;
using WanderVerse.Backend; 
using Debug = UnityEngine.Debug; // Added to prevent conflict with System.Diagnostics

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
            PlayFromUnityEditor(); // Randiv's Quality of Life trigger
        }

        // This is erased when building the APK. It lets Theshara/Vihanga test levels directly.
        [Conditional("UNITY_EDITOR")]
        private void PlayFromUnityEditor()
        {
            if (CurrentData == null)
            {
                Debug.LogWarning("[DEV Sync] Direct scene play detected! Auto-initializing as Guest to enable saving.");
                InitializeAsGuest();
            }
        }
        
        public void InitializeAsGuest()
        {
            Debug.Log("[Sync] Initializing Guest Mode...");
            IsGuest = true;

            if (LocalDataManager.Instance != null)
            {
                // Tell LocalDataManager to save unencrypted guest data
                LocalDataManager.Instance.EnableGuestMode(); 
            }

            // Load Local Data
            PlayerData local = LocalDataManager.Instance.LoadGame();

            if (local != null)
            {
                CurrentData = local;
                Debug.Log($"[Sync] Guest Data Loaded. XP: {CurrentData.xp}");
            }
            else
            {
                CurrentData = new PlayerData { userID = "guest_" + System.Guid.NewGuid().ToString() };
                
                LocalDataManager.Instance.SaveGame(CurrentData);
                
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

            PlayerData local = LocalDataManager.Instance.LoadGame();

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
            
            LocalDataManager.Instance.SaveGame(CurrentData); 

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
                    
                    LocalDataManager.Instance.SaveGame(CurrentData);
                }
            }
            else
            {
                // New User logic
                if (CurrentData == null) 
                {
                    CurrentData = new PlayerData { userID = userId };
                    Debug.Log("[Cloud] New User. Created New Profile.");
                                        
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

        // World Map unlock logic for Seshani's UI
        public bool IsLevelUnlocked(string levelID)
        {
            if (CurrentData == null) return false;

            LevelTracker tracker = CurrentData.GetLevelData(levelID);
            
            if (tracker != null) 
            {
                return tracker.isUnlocked;
            }

            // The first level is always unlocked for new players
            if (levelID == "Island1_Lesson1") 
            {
                return true;
            }

            return false;
        }

        public int GetStarsForLevel(string levelID)
        {
            if (CurrentData == null) return 0;

            LevelTracker tracker = CurrentData.GetLevelData(levelID);
            return tracker != null ? tracker.starsEarned : 0;
        }

        public void UnlockLevel(string levelID)
        {
            if (CurrentData == null) return;

            LevelTracker tracker = CurrentData.GetLevelData(levelID);

            if (tracker != null)
            {
                tracker.isUnlocked = true;
            }
            else
            {
                LevelTracker newTracker = new LevelTracker
                {
                    levelID = levelID,
                    highScore = 0,
                    starsEarned = 0,
                    attempts = 0,
                    isUnlocked = true
                };
                CurrentData.levelProgress.Add(newTracker);
            }

            SyncProgress(CurrentData); 
            Debug.Log($"[WorldMap] Unlocked Level: {levelID}");
        }
    }

    public class DashboardUIController : MonoBehaviour
    {
        [Header("Top Profile")]
        public TextMeshProUGUI usernameText;
        public TextMeshProUGUI rankText; // Calculated based on XP
        public TextMeshProUGUI streakText;

        [Header("Progress Bars")]
        public Slider subjectProgressBar; // Overall Maths progress
        public TextMeshProUGUI map1PercentageText; // e.g., "Island 1: 80%"
        
        [Header("Badges")]
        public GameObject[] badgeIcons; // Seshani drags badge images here

        void OnEnable() 
        {
            UpdateDashboard();
        }

        public void UpdateDashboard()
        {
            var data = CloudSyncManager.Instance.CurrentData;
            if (data == null) return;

            // 1. Basic Info
            usernameText.text = data.userName;
            streakText.text = "Login Streak: " + CalculateStreak(data) + " Days";
            rankText.text = GetRankName(data.xp);

            // 2. Map & Subject Progress
            float totalProgress = CalculateSubjectProgress(data);
            subjectProgressBar.value = totalProgress;
            
            map1PercentageText.text = $"Island 1: {(totalProgress * 100):F0}%";

            // 3. Badges (Simple Logic: If XP > 500, show Badge 0)
            badgeIcons[0].SetActive(data.xp > 500);
            badgeIcons[1].SetActive(data.currentLevel > 5);
        }

        private float CalculateSubjectProgress(PlayerData data)
        {
            if (data.levelProgress.Count == 0) return 0;
            
            int completed = 0;
            foreach (var level in data.levelProgress)
            {
                if (level.starsEarned > 0) completed++;
            }
            // Assuming there are 12 levels total for the subject
            return (float)completed / 12f; 
        }

        private string GetRankName(int xp)
        {
            if (xp < 100) return "Novice Explorer";
            if (xp < 500) return "Junior Wanderer";
            return "Master Pathfinder";
        }

        private int CalculateStreak(PlayerData data)
        {
            // Placeholder: You'll need to compare lastDailyResetTimestamp with current time
            return 1; 
        }
    }
}