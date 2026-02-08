using UnityEngine;
using TMPro;                
using Firebase.Auth;        
using UnityEngine.SceneManagement; 
using System.Collections;
using System.Threading.Tasks; 
using System;                 
using WanderVerse.Framework.Data; 

namespace WanderVerse.Backend.Services
{
    public class AuthManager : MonoBehaviour
    {
        [Header("Login Panel References")]
        public TMP_InputField loginEmailInput;
        public TMP_InputField loginPasswordInput;

        [Header("Sign Up Panel References")]
        public TMP_InputField signUpEmailInput;
        public TMP_InputField signUpPasswordInput;
        public TMP_InputField signUpConfirmPasswordInput; 

        [Header("General UI")]
        public TextMeshProUGUI feedbackText; 

        private FirebaseAuth _auth;
        private bool _isWorking = false; 

        void Start()
        {
            _auth = FirebaseAuth.DefaultInstance;
    
            if(feedbackText != null) feedbackText.text = "";

            if (_auth.CurrentUser != null)
            {
                Debug.Log($"[Auth] Welcome back, {_auth.CurrentUser.UserId}");
                LoadWorldMap();
            }
            
        }
        
        public void OnSignUpButton() 
        {
            string p1 = signUpPasswordInput.text;
            string p2 = signUpConfirmPasswordInput.text;

            if (string.IsNullOrEmpty(p1))
            {
                UpdateFeedback("Password cannot be empty.");
                return;
            }

            if (p1 != p2)
            {
                UpdateFeedback("Passwords do not match!");
                return; 
            }

            StartCoroutine(AuthRoutine(
                () => _auth.CreateUserWithEmailAndPasswordAsync(signUpEmailInput.text, p1), true));
        }

        public void OnLoginButton() => StartCoroutine(AuthRoutine(
            () => _auth.SignInWithEmailAndPasswordAsync(loginEmailInput.text, loginPasswordInput.text), false));

        public void OnGuestLoginButton() => StartCoroutine(AuthRoutine(
            () => _auth.SignInAnonymouslyAsync(), true));


        private IEnumerator AuthRoutine(Func<Task<AuthResult>> authTask, bool isNewUser)
        {
            if (_isWorking) yield break;
            _isWorking = true;
            UpdateFeedback("Processing...");

            var task = authTask();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null) 
            {
                Debug.LogError($"Auth Error: {task.Exception.InnerException?.Message}");
                UpdateFeedback($"Error: {task.Exception.InnerException?.Message}");
                _isWorking = false;
            } 
            else 
            {
                UpdateFeedback("Success!");
                string uid = task.Result.User.UserId;

                if (isNewUser) InitializeNewUserData(uid);
                LoadWorldMap();
            }
        }

        private void InitializeNewUserData(string uid)
        {
            PlayerData newData = new PlayerData { userID = uid };
            // TO DO: @Senmith - Uncomment this line when LocalDataManager is ready
            // LocalDataManager.Save(newData); 
            Debug.Log("[Auth] Created local data (InMemory only).");
        }

        private void LoadWorldMap()
        {
            if (CloudSyncManager.Instance != null)
                CloudSyncManager.Instance.InitializeData(_auth.CurrentUser.UserId);
            
            SceneManager.LoadScene("Scene_WorldMap");
        }

        private void UpdateFeedback(string msg) { if (feedbackText) feedbackText.text = msg; }
    }
}