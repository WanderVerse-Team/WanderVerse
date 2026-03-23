using UnityEngine;
using TMPro;                
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine.SceneManagement; 
using System.Collections;
using System.Threading.Tasks; 
using System;
using System.Collections.Generic; 
using WanderVerse.Framework.Data; 

namespace WanderVerse.Backend.Services
{
    public class AuthManager : MonoBehaviour
    {
        [Header("Login Panel References")]
        public TMP_InputField loginEmailInput; // Accepts Username OR Email
        public TMP_InputField loginPasswordInput;

        [Header("Sign Up Panel References")]
        public TMP_InputField signUpUsernameInput;      
        public TMP_InputField signUpEmailInput; 
        public TMP_InputField signUpPasswordInput;
        public TMP_InputField signUpConfirmPasswordInput; 

        [Header("Feedback UI")]
        public TextMeshProUGUI signInFeedbackText;
        public TextMeshProUGUI signUpFeedbackText;

        [Header("UI Panels")]
        public GameObject panelSignIn; 
        public GameObject panelSignUp; 

        private FirebaseAuth _auth;
        private FirebaseFirestore _db; 
        private bool _isWorking = false;
        private bool _firebaseReady = false;
        private bool _isRedirecting = false;
        private bool _didInitialSessionCheck = false;

        void Start()
        {
            StartCoroutine(InitializeFirebaseAndSetup());

            if (loginEmailInput != null)
                loginEmailInput.onValueChanged.AddListener(delegate { ClearSignInError(); });

            if (loginPasswordInput != null)
                loginPasswordInput.onValueChanged.AddListener(delegate { ClearSignInError(); });

            if (signUpUsernameInput != null)
                signUpUsernameInput.onValueChanged.AddListener(delegate { ClearSignUpError(); });

            if (signUpEmailInput != null)
                signUpEmailInput.onValueChanged.AddListener(delegate { ClearSignUpError(); });

            if (signUpPasswordInput != null)
                signUpPasswordInput.onValueChanged.AddListener(delegate { ClearSignUpError(); });

            if (signUpConfirmPasswordInput != null)
                signUpConfirmPasswordInput.onValueChanged.AddListener(delegate { ClearSignUpError(); });
        }

        private IEnumerator InitializeFirebaseAndSetup()
        {
            if (signInFeedbackText != null) signInFeedbackText.text = "Initializing...";
            if (signUpFeedbackText != null) signUpFeedbackText.text = "Initializing...";

            // Wait for Firebase to be ready
            var checkTask = FirebaseApp.CheckAndFixDependenciesAsync();
            yield return new WaitUntil(() => checkTask.IsCompleted);

            if (checkTask.Result != DependencyStatus.Available)
            {
                Debug.LogError($"[Auth] Firebase not available: {checkTask.Result}");
                if (signInFeedbackText != null) signInFeedbackText.text = "Firebase Error. Please restart.";
                if (signUpFeedbackText != null) signUpFeedbackText.text = "Firebase Error. Please restart.";
                yield break;
            }

            // Now safely initialize Firebase services
            _auth = FirebaseAuth.DefaultInstance;
            _db = FirebaseFirestore.DefaultInstance;
            _auth.StateChanged += OnAuthStateChanged;
            _firebaseReady = true;

            Debug.Log("[Auth] Firebase initialized successfully.");
            SetupUI();
        }

        private void OnDestroy()
        {
            if (_auth != null)
            {
                _auth.StateChanged -= OnAuthStateChanged;
            }
        }

        private void OnAuthStateChanged(object sender, EventArgs eventArgs)
        {
            if (!_firebaseReady || _auth == null || _isRedirecting)
            {
                return;
            }

            if (_auth.CurrentUser != null)
            {
                Debug.Log($"[Auth] Session restored for {_auth.CurrentUser.UserId}");
                BeginAutoLogin();
            }
        }

        private void SetupUI()
        {
            if (signInFeedbackText != null) signInFeedbackText.text = "";
            if (signUpFeedbackText != null) signUpFeedbackText.text = "";

            if (signUpUsernameInput != null) signUpUsernameInput.readOnly = true;

            // HARD-WIRE ALL BUTTONS
            // IMPORTANT: Activate both panels before setting up buttons
            if (panelSignIn != null) panelSignIn.SetActive(true);
            if (panelSignUp != null) panelSignUp.SetActive(true);

            SetupButton("Generate_Username", OnGenerateUsernameButton);
            SetupButton("Btn_SignUp", OnSignUpButton);
            SetupButton("Btn_SignIn", OnLoginButton);
            SetupButton("Btn_Guest", OnGuestLoginButton);

            SetupButton("Btn_GoToSignUp", () => 
            { 
                ClearSignInError();
                ClearSignUpError();
                panelSignIn.SetActive(false); 
                panelSignUp.SetActive(true); 
            });

            SetupButton("Btn_GoToSignIn", () => 
            { 
                ClearSignInError();
                ClearSignUpError();
                panelSignUp.SetActive(false); 
                panelSignIn.SetActive(true); 
            });

            Debug.Log("<color=cyan>[Auth] All UI listeners hard-wired via code.</color>");

            // Hide auth panels while we quickly check for a persisted Firebase session.
            if (panelSignIn != null) panelSignIn.SetActive(false);
            if (panelSignUp != null) panelSignUp.SetActive(false);

            StartCoroutine(InitialSessionCheckRoutine());
        }

        private IEnumerator InitialSessionCheckRoutine()
        {
            UpdateFeedback("Checking saved session...", false);

            float timeout = 2.5f;
            float elapsed = 0f;

            while (_auth != null && _auth.CurrentUser == null && elapsed < timeout)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            _didInitialSessionCheck = true;

            if (_auth != null && _auth.CurrentUser != null)
            {
                BeginAutoLogin();
                yield break;
            }

            ShowAuthPanels();
            UpdateFeedback("WanderVerse Ready...", false);
        }

        private void BeginAutoLogin()
        {
            if (_isRedirecting)
            {
                return;
            }

            UpdateFeedback("Welcome back! Signing you in...", false);
            Debug.Log($"[Auth] Welcome back, {_auth.CurrentUser.UserId}");
            LoadWorldMap();
        }

        private void ShowAuthPanels()
        {
            // Keep existing UX: show SignUp first and allow switching to SignIn via button.
            if (panelSignIn != null) panelSignIn.SetActive(false);
            if (panelSignUp != null) panelSignUp.SetActive(true);
        }

        // A helper method to keep Start() clean
            if (panelSignUp != null) panelSignUp.SetActive(true);
        }

        private void SetupButton(string gameObjectName, UnityEngine.Events.UnityAction action)
        {
            GameObject btnObj = GameObject.Find(gameObjectName);
            if (btnObj != null)
            {
                UnityEngine.UI.Button btn = btnObj.GetComponent<UnityEngine.UI.Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(action);
                    Debug.Log($"<color=green>[Auto-Link] Successfully linked {gameObjectName}</color>");
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow>[Auto-Link] Optional button '{gameObjectName}' not found in this panel.</color>");
            }
        }

        public void OnGenerateUsernameButton()
        {
            Debug.Log("!!! THE BUTTON IS WORKING !!!");
            string[] adjectives = {"Chaotic","Vibey","Iconic","Unhinged","Whimsical","Electric","Cosmic","Quirky","Sneaky", 
                                    "Crispy","Glitchy","Moody","Radiant","Ferocious","Sleepy","Spicy","Dreamy","Mysterious",
                                    "Goofy","Silly","Unstoppable","Legendary","Petty","Bold","Dramatic","Playful","Dazzling",
                                    "Noisy","Silent","Curious","Bubbly","Savage","Cozy","Sharp","Feral","Magical","Hyper",
                                    "Caffeinated","Nervous","Fearless","Soft","Spooky","Luminous","Frosty","Fiery","Witty",
                                    "Compact","Glorious","Blurry","Funky","Swirly","Crunchy","Juicy","Sparkly","Slippery",
                                    "Brainy","Agile","Stubborn","Restless","Slick","Messy","Polished","Loud","Subtle","Wild",
                                    "Cursed","Blessed","Underrated","Overcaffeinated","Pixelated","Sentient","Shiny",
                                    "Aesthetic","Unpredictable","Random","Golden","Animated","Timeless","Fuzzy","Vivid",
                                    "Minimal","Epic","Tiny","Massive","Lowkey","Highkey","Unbothered","LockedIn","Bizarre",
                                    "Nostalgic","Suspicious","Energetic","ChaoticGood","ElectricCore","Frostbitten",
                                    "Hyperactive","Zen","Muted","Saturated","Turbo","SleepDeprived","Focused","Clumsy",
                                    "Snappy","Elastic","Plastic","Metallic","Digital","Analog","Retro","Futureproof",
                                    "Neon","Pastel","Grim","Sunny","Stormy","Breezy","Dusty","Icy","Toasty","Minty",
                                    "Salty","Sweet","Sour","Bitter","Hollow","Dense","Featherlight","Heavyweight",
                                    "Rugged","Smooth","Velvet","Glossy","Matte","Nocturnal","Diurnal","Ambient",
                                    "Reactive","Static","Dynamic","Recursive","Linear","Parallel","Asynchronous",
                                    "Optimized","Unoptimized","Hacky","Clean","MessFree","Overengineered",
                                    "Underbaked","BattleTested","Experimental","Stable","Volatile","Resilient",
                                    "Fragile","Modular","Scalable","Distributed","Centralized","Encrypted",
                                    "Unlocked","Hidden","Exposed","Curated","Raw","Unfiltered","Infinite","Finite",
                                    "Quantum","Atomic","Cosmological","Mythic","Arcane","Sacred","Profane","Edgy",
                                    "Cracked","Based","Chill","LockedOn" };

            string[] nouns = {"Goblin","Pixel","Vibe","Algorithm","Banana","Pancake","Wizard","Portal","Chaos","Spark",
                                    "Cloud","Gremlin","Frog","Dragon","Orbit","Echo","Shadow","Cookie","Circuit","Glitch",
                                    "Meme","Asteroid","Button","Bubble","Rocket","Storm","Comet","Notebook","Lantern","Ghost",
                                    "Penguin","Toast","Kraken","Satellite","Compass","Code","Dungeon","Phoenix","Scroll",
                                    "Potion","Map","Quest","Backpack","Planet","Cupcake","Server","Wizardry","Hoodie","Helmet",
                                    "Star","Void","Flame","Battery","Keyboard","Signal","Pixelstorm","Moonbeam","SideQuest",
                                    "Braincell","Spreadsheet","Token","Coin","Relic","Badge","Artifact","Timeline","Thread",
                                    "Notification","Shortcut","Bug","Feature","Update","Patch","Commit","Branch","Merge","Repo",
                                    "Terminal","Cache","Avatar","Username","Handle","Playlist","Sticker","Emoji","Brainwave",
                                    "Wormhole","Dashboard","Blueprint","Sandbox","Vault","Beacon","Catalyst","Switch",
                                    "Engine","Kernel","Process","Threadpool","Daemon","Pipeline","Queue","Stack","Heap",
                                    "Pointer","Reference","Object","Instance","Module","Package","Library","Framework",
                                    "Protocol","Packet","Socket","Port","Gateway","Router","Firewall","Proxy","Mirror",
                                    "Index","Shard","Cluster","Node","Ledger","Block","Chain","Hash","Salt","Nonce",
                                    "Signature","Certificate","Keypair","TokenizedAsset","Wallet","Gas","Oracle",
                                    "Contract","Epoch","Timestamp","Clock","Scheduler","Timer","Interrupt","SignalBus",
                                    "Event","Listener","Emitter","Stream","Buffer","Cacheline","Page","Sector","Disk",
                                    "Volume","Snapshot","Backup","Restore","Checkpoint","Rollback","Diff","Patchset",
                                    "Changelog","Roadmap","Milestone","Sprint","Backlog","Ticket","Issue","Comment",
                                    "Reaction","Mention","Threadlock","Archive","Fork","Upstream","Downstream",
                                    "Namespace","Identifier","Symbol","Literal","Opcode","Instruction","Register",
                                    "Core","Chip","Die","Wafer","Fabric","Mesh"};

            Debug.Log("System: Generate Button was pressed!"); 

            if (signUpUsernameInput == null)
            {
                Debug.LogError("System: You haven't linked the InputField to _AppSystems!");
                return;
            }
            
            string randomAdj = adjectives[UnityEngine.Random.Range(0, adjectives.Length)];
            string randomNoun = nouns[UnityEngine.Random.Range(0, nouns.Length)];

            signUpUsernameInput.text = $"{randomAdj}{randomNoun}";
            signUpUsernameInput.ForceLabelUpdate(); 

            UpdateSignUpFeedback("Username generated!", false);
        }
        
        public void OnSignUpButton() 
        {
            if (!_firebaseReady) 
            { 
                UpdateSignUpFeedback("Firebase not ready. Please wait...", true); 
                return; 
            }

            string p1 = signUpPasswordInput.text;
            string p2 = signUpConfirmPasswordInput.text;
            string uName = signUpUsernameInput.text;
            string emailInput = signUpEmailInput.text;

            if (string.IsNullOrEmpty(uName)) 
            { 
                UpdateSignUpFeedback("Please generate a username.", true); 
                return; 
            }

            if (string.IsNullOrEmpty(p1)) 
            { 
                UpdateSignUpFeedback("Password cannot be empty.", true); 
                return; 
            }

            if (p1 != p2) 
            { 
                UpdateSignUpFeedback("Passwords do not match.", true); 
                return; 
            }

            string finalEmail = emailInput;
            if (string.IsNullOrEmpty(finalEmail))
            {
                finalEmail = $"{uName}@wanderverse.wuaze.com";
            }

            StartCoroutine(SignUpRoutine(uName, finalEmail, p1));
        }

        public void OnLoginButton() 
        {
            if (!_firebaseReady) 
            { 
                UpdateSignInFeedback("Firebase not ready. Please wait...", true); 
                return; 
            }

            string input = loginEmailInput.text;
            string pass = loginPasswordInput.text;

            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(pass)) 
            {
                UpdateSignInFeedback("Username or password incorrect. Please enter again", true);
                return;
            }

            if (input.Contains("@"))
            {
                StartCoroutine(LoginRoutine(input, pass));
            }
            else
            {
                StartCoroutine(UsernameLoginRoutine(input, pass));
            }
        }

        public void OnGuestLoginButton() 
        {
            if (!_firebaseReady) 
            { 
                UpdateSignInFeedback("Please wait...", false); 
                return; 
            }

            UpdateSignInFeedback("Starting Offline Mode...", false);
            
            if (CloudSyncManager.Instance != null)
            {
                CloudSyncManager.Instance.InitializeAsGuest();

                if (CloudSyncManager.Instance.CurrentData != null && 
                    CloudSyncManager.Instance.CurrentData.hasCompletedOnboarding)
                {
                    Debug.Log("[Auth] Guest returning. Loading World Map.");
                    SceneManager.LoadScene("Scene_WorldMap");
                }
                else
                {
                    Debug.Log("[Auth] New Guest. Loading Onboarding.");
                    SceneManager.LoadScene("Scene_GradeSelection");
                }
            }
            else
            {
                Debug.LogWarning("[Auth] CloudSyncManager missing!");
                SceneManager.LoadScene("Scene_GradeSelection");
            }
        }

        public void ResetPassword()
        {
            string input = loginEmailInput.text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                UpdateSignInFeedback("Enter your email to reset password.", true);
                return;
            }

            if (!input.Contains("@"))
            {
                UpdateSignInFeedback("Password reset is only available via linked Email accounts.", true);
                return;
            }

            if (input.EndsWith("@wanderverse.wuaze.com")) 
            {
                UpdateSignInFeedback("This account does not have a recovery email linked.", true);
                return;
            }

            StartCoroutine(PerformPasswordReset(input));
        }

        private IEnumerator PerformPasswordReset(string email)
        {
            UpdateSignInFeedback("Sending reset link...", false);

            var task = _auth.SendPasswordResetEmailAsync(email);
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null)
            {
                UpdateSignInFeedback("Account not found or invalid email.", true);
            }
            else
            {
                UpdateSignInFeedback("Recovery email sent! Check your inbox.", false);
            }
        }

        private IEnumerator SignUpRoutine(string username, string email, string password)
        {
            if (_isWorking) yield break;
            _isWorking = true;
            
            UpdateSignUpFeedback("Checking availability...", false);
            var checkTask = _db.Collection("usernames").Document(username).GetSnapshotAsync();
            yield return new WaitUntil(() => checkTask.IsCompleted);

            if (checkTask.Result.Exists)
            {
                UpdateSignUpFeedback("Username already taken. Please try again.", true);
                _isWorking = false;
                yield break;
            }

            UpdateSignUpFeedback("Creating account...", false);
            var authTask = _auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => authTask.IsCompleted);

            if (authTask.Exception != null) 
            {
                UpdateSignUpFeedback("Sign up failed. Please check your details and try again.", true);
                _isWorking = false;
            } 
            else 
            {
                string uid = authTask.Result.User.UserId;
                yield return StartCoroutine(RegisterUsernameInDB(username, email, uid));
                InitializeNewUserData(uid);

                UpdateSignUpFeedback("Account created!", false);
                yield return new WaitForSeconds(1.5f); 

                _auth.SignOut(); 
                
                if (panelSignUp != null) panelSignUp.SetActive(false);
                if (panelSignIn != null) panelSignIn.SetActive(true);

                if (loginEmailInput != null) loginEmailInput.text = username; 

                UpdateSignInFeedback("Success! Please log in.", false);
                _isWorking = false; 
            }
        }

        private IEnumerator RegisterUsernameInDB(string username, string email, string uid)
        {
            var data = new Dictionary<string, object>
            {
                { "email", email },
                { "uid", uid }
            };

            var saveTask = _db.Collection("usernames").Document(username).SetAsync(data);
            yield return new WaitUntil(() => saveTask.IsCompleted);
        }

        private IEnumerator UsernameLoginRoutine(string username, string password)
        {
            if (_isWorking) yield break;
            _isWorking = true;
            UpdateSignInFeedback("Verifying username...", false);

            var docTask = _db.Collection("usernames").Document(username).GetSnapshotAsync();
            yield return new WaitUntil(() => docTask.IsCompleted);

            if (!docTask.Result.Exists)
            {
                UpdateSignInFeedback("Username or password incorrect. Please enter again", true);
                _isWorking = false;
                yield break;
            }

            string realEmail = docTask.Result.GetValue<string>("email");
            UpdateSignInFeedback("Authenticating...", false);

            var authTask = _auth.SignInWithEmailAndPasswordAsync(realEmail, password);
            yield return new WaitUntil(() => authTask.IsCompleted);

            if (authTask.Exception != null)
            {
                UpdateSignInFeedback("Username or password incorrect. Please enter again", true);                
                _isWorking = false;
            }
            else
            {
                UpdateSignInFeedback("Welcome!", false);
                LoadWorldMap();
            }
        }

        private IEnumerator LoginRoutine(string email, string password)
        {
            if (_isWorking) yield break;
            _isWorking = true;
            UpdateSignInFeedback("Logging in...", false);

            var authTask = _auth.SignInWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => authTask.IsCompleted);

            if (authTask.Exception != null)
            {
                Debug.LogWarning($"[Auth] Login Failed: {authTask.Exception.InnerException?.Message}");
                UpdateSignInFeedback("Username or password incorrect. Please enter again", true);
                _isWorking = false;
            }
            else
            {
                UpdateSignInFeedback("Login Successful!", false);
                LoadWorldMap();
            }
        }

        private IEnumerator AuthRoutine(Func<Task<AuthResult>> authTask, bool isNewUser)
        {
            if (_isWorking) yield break;
            _isWorking = true;
            UpdateSignInFeedback("Processing...", false);

            var task = authTask();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null) 
            {
                UpdateSignInFeedback("Something went wrong. Please try again.", true);
                _isWorking = false;
            } 
            else 
            {
                UpdateSignInFeedback("Success!", false);
                string uid = task.Result.User.UserId;
                if (isNewUser) InitializeNewUserData(uid);
                LoadWorldMap();
            }
        }

        private void InitializeNewUserData(string uid)
        {
            string finalUsername = signUpUsernameInput.text;
            
            if (string.IsNullOrEmpty(finalUsername)) finalUsername = "Traveler_" + uid.Substring(0, 4);

            PlayerData dataToSave;

            if (CloudSyncManager.Instance != null && CloudSyncManager.Instance.CurrentData != null)
            {
                dataToSave = CloudSyncManager.Instance.CurrentData;
                dataToSave.userID = uid;
                dataToSave.userName = finalUsername;
                Debug.Log($"[Auth] Converted Guest to User. Preserving XP: {dataToSave.xp}");
            }
            else
            {
                dataToSave = new PlayerData 
                { 
                    userID = uid,
                    userName = finalUsername 
                };
                Debug.Log($"[Auth] Created fresh user: {finalUsername}");
            }

            if (CloudSyncManager.Instance != null)
            {
                CloudSyncManager.Instance.SyncProgress(dataToSave); 
            }
        }

        private void LoadWorldMap()
        {
            if (_isRedirecting)
            {
                return;
            }

            _isRedirecting = true;

            if (_auth == null || _auth.CurrentUser == null)
            {
                _isRedirecting = false;
                if (_didInitialSessionCheck)
                {
                    ShowAuthPanels();
                }
                return;
            }

            if (CloudSyncManager.Instance == null)
            {
                Debug.LogWarning("[Auth] CloudSyncManager missing. Loading World Map directly.");
                _isRedirecting = false;
                SceneManager.LoadScene("Scene_WorldMap");
                return;
            }

            if (CloudSyncManager.Instance != null)
            {
                // Tell CloudSyncManager to fetch the user data
                CloudSyncManager.Instance.InitializeAsUser(_auth.CurrentUser.UserId);
                
                // Start a Coroutine to wait for the data to arrive before switching scenes
                CloudSyncManager.Instance.InitializeAsUser(_auth.CurrentUser.UserId);
                StartCoroutine(WaitForDataAndRedirect());
            }
        }

        private IEnumerator WaitForDataAndRedirect()
        {
            yield return new WaitUntil(() => CloudSyncManager.Instance.CurrentData != null);

            PlayerData data = CloudSyncManager.Instance.CurrentData;

            if (data.hasCompletedOnboarding)
            {
                Debug.Log("[Auth] User has completed onboarding. Loading World Map...");
                SceneManager.LoadScene("Scene_WorldMap");
            }
            else
            {
                Debug.Log("[Auth] New User detected. Loading Grade Selection...");
                SceneManager.LoadScene("Scene_GradeSelection");
            }

            _isRedirecting = false;
        }

        private void UpdateSignInFeedback(string msg, bool isError = false)
        {
            if (signInFeedbackText != null)
            {
                signInFeedbackText.text = msg;
                signInFeedbackText.color = isError ? Color.red : Color.white;
            }
        }

        private void UpdateSignUpFeedback(string msg, bool isError = false)
        {
            if (signUpFeedbackText != null)
            {
                signUpFeedbackText.text = msg;
                signUpFeedbackText.color = isError ? Color.red : Color.white;
            }
        }

        private void ClearSignInError()
        {
            UpdateSignInFeedback("", false);
        }

        private void ClearSignUpError()
        {
            UpdateSignUpFeedback("", false);
        }
    }
}