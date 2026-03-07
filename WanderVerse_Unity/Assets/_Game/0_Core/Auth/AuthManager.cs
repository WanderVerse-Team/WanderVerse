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

        [Header("General UI")]
        public TextMeshProUGUI feedbackText; 

        [Header("UI Panels")]
        public GameObject panelSignIn; 
        public GameObject panelSignUp; 

        private FirebaseAuth _auth;
        private FirebaseFirestore _db; 
        private bool _isWorking = false;
        private bool _firebaseReady = false;

        void Start()
        {
            StartCoroutine(InitializeFirebaseAndSetup());
        }

        private IEnumerator InitializeFirebaseAndSetup()
        {
            if (feedbackText != null) feedbackText.text = "Initializing...";

            // Wait for Firebase to be ready
            var checkTask = FirebaseApp.CheckAndFixDependenciesAsync();
            yield return new WaitUntil(() => checkTask.IsCompleted);

            if (checkTask.Result != DependencyStatus.Available)
            {
                Debug.LogError($"[Auth] Firebase not available: {checkTask.Result}");
                if (feedbackText != null) feedbackText.text = "Firebase Error. Please restart.";
                yield break;
            }

            // Now safely initialize Firebase services
            _auth = FirebaseAuth.DefaultInstance;
            _db = FirebaseFirestore.DefaultInstance;
            _firebaseReady = true;

            Debug.Log("[Auth] Firebase initialized successfully.");
            SetupUI();
        }

        private void SetupUI()
        {

            if (feedbackText != null) feedbackText.text = "WanderVerse Ready...";

            if (signUpUsernameInput != null) signUpUsernameInput.readOnly = true;

            // 2. HARD-WIRE ALL BUTTONS
            // IMPORTANT: Activate both panels before setting up buttons
            // GameObject.Find() only works on active GameObjects
            if (panelSignIn != null) panelSignIn.SetActive(true);
            if (panelSignUp != null) panelSignUp.SetActive(true);

            SetupButton("Generate_Username", OnGenerateUsernameButton);
            SetupButton("Btn_SignUp", OnSignUpButton);
            SetupButton("Btn_SignIn", OnLoginButton);
            SetupButton("Btn_Guest", OnGuestLoginButton);

            SetupButton("Btn_GoToSignUp", () => { panelSignIn.SetActive(false); panelSignUp.SetActive(true); });
            SetupButton("Btn_GoToSignIn", () => { panelSignUp.SetActive(false); panelSignIn.SetActive(true); });

            Debug.Log("<color=cyan>[Auth] All UI listeners hard-wired via code.</color>");

            // Now set initial panel visibility (show SignUp, hide SignIn by default)
            if (panelSignIn != null) panelSignIn.SetActive(false);
            if (panelSignUp != null) panelSignUp.SetActive(true);

            // Auto logs in a user
            /*if (_auth.CurrentUser != null)
            {
                Debug.Log($"[Auth] Welcome back, {_auth.CurrentUser.UserId}");
                LoadWorldMap();
            }*/
        }

        // A helper method to keep your Start() clean
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

            // This will appear in the Console in WHITE text
            Debug.Log("System: Generate Button was pressed!"); 

            if (signUpUsernameInput == null)
            {
                // This will appear in RED text if the Inspector is empty
                Debug.LogError("System: You haven't linked the InputField to _AppSystems!");
                return;
            }
            
            string randomAdj = adjectives[UnityEngine.Random.Range(0, adjectives.Length)];
            string randomNoun = nouns[UnityEngine.Random.Range(0, nouns.Length)];

            signUpUsernameInput.text = $"{randomAdj}{randomNoun}";
            
            signUpUsernameInput.ForceLabelUpdate(); 
            UpdateFeedback("Username generated!");
        }
        
        public void OnSignUpButton() 
        {
            if (!_firebaseReady) { UpdateFeedback("Firebase not ready. Please wait..."); return; }

            string p1 = signUpPasswordInput.text;
            string p2 = signUpConfirmPasswordInput.text;
            string uName = signUpUsernameInput.text;
            string emailInput = signUpEmailInput.text;

            if (string.IsNullOrEmpty(uName)) { UpdateFeedback("Please generate a username."); return; }
            if (string.IsNullOrEmpty(p1)) { UpdateFeedback("Password cannot be empty."); return; }
            if (p1 != p2) { UpdateFeedback("Passwords do not match!"); return; }

            string finalEmail = emailInput;
            if (string.IsNullOrEmpty(finalEmail))
            {
                finalEmail = $"{uName}@wanderverse.wuaze.com";
            }

            StartCoroutine(SignUpRoutine(uName, finalEmail, p1));
        }

        public void OnLoginButton() 
        {
            if (!_firebaseReady) { UpdateFeedback("Firebase not ready. Please wait..."); return; }

            string input = loginEmailInput.text;
            string pass = loginPasswordInput.text;

            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(pass)) 
            {
                UpdateFeedback("Please fill all fields.");
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
            // Guest mode doesn't require Firebase, but we wait for it anyway for consistency
            if (!_firebaseReady) { UpdateFeedback("Please wait..."); return; }

            UpdateFeedback("Starting Offline Mode...");
            
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


        private IEnumerator SignUpRoutine(string username, string email, string password)
        {
            if (_isWorking) yield break;
            _isWorking = true;
            
            UpdateFeedback("Checking availability...");
            var checkTask = _db.Collection("usernames").Document(username).GetSnapshotAsync();
            yield return new WaitUntil(() => checkTask.IsCompleted);

            if (checkTask.Result.Exists)
            {
                UpdateFeedback($"'{username}' is taken! Try another.");
                _isWorking = false;
                yield break;
            }

            UpdateFeedback("Creating Account...");
            var authTask = _auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => authTask.IsCompleted);

            if (authTask.Exception != null) 
            {
                UpdateFeedback($"Error: {authTask.Exception.InnerException?.Message}");
                _isWorking = false;
            } 
            else 
            {
                string uid = authTask.Result.User.UserId;
                yield return StartCoroutine(RegisterUsernameInDB(username, email, uid));
                InitializeNewUserData(uid);

                UpdateFeedback("Account Created! Redirecting...");
                yield return new WaitForSeconds(1.5f); 

                _auth.SignOut(); 
                
                if (panelSignUp != null) panelSignUp.SetActive(false);
                if (panelSignIn != null) panelSignIn.SetActive(true);

                if (loginEmailInput != null) loginEmailInput.text = username; 

                UpdateFeedback("Success! Please log in.");
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
            UpdateFeedback("Looking up account...");

            // Finds the email for this username
            var docTask = _db.Collection("usernames").Document(username).GetSnapshotAsync();
            yield return new WaitUntil(() => docTask.IsCompleted);

            if (!docTask.Result.Exists)
            {
                UpdateFeedback("Username not found.");
                _isWorking = false;
                yield break;
            }

            // Get the real email (or shadow email)
            string realEmail = docTask.Result.GetValue<string>("email");

            // Login with that email
            var authTask = _auth.SignInWithEmailAndPasswordAsync(realEmail, password);
            yield return new WaitUntil(() => authTask.IsCompleted);

            if (authTask.Exception != null)
            {
                UpdateFeedback("Login Failed. Check password.");
                _isWorking = false;
            }
            else
            {
                UpdateFeedback("Success!");
                LoadWorldMap();
            }
        }

        private IEnumerator LoginRoutine(string email, string password)
        {
            if (_isWorking) yield break;
            _isWorking = true;
            UpdateFeedback("Logging in...");

            var authTask = _auth.SignInWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => authTask.IsCompleted);

            if (authTask.Exception != null)
            {
                UpdateFeedback($"Error: {authTask.Exception.InnerException?.Message}");
                _isWorking = false;
            }
            else
            {
                UpdateFeedback("Success!");
                LoadWorldMap();
            }
        }

        private IEnumerator AuthRoutine(Func<Task<AuthResult>> authTask, bool isNewUser)
        {
            if (_isWorking) yield break;
            _isWorking = true;
            UpdateFeedback("Processing...");

            var task = authTask();
            yield return new WaitUntil(() => task.IsCompleted);

            if (task.Exception != null) 
            {
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
            string finalUsername = signUpUsernameInput.text;
            
            if (string.IsNullOrEmpty(finalUsername)) finalUsername = "Traveler_" + uid.Substring(0, 4);

            PlayerData dataToSave;

            // Checks if we already have Guest data in memory
            if (CloudSyncManager.Instance != null && CloudSyncManager.Instance.CurrentData != null)
            {
                // If we have guest data, just attach the new UserID and Name to it.
                dataToSave = CloudSyncManager.Instance.CurrentData;
                dataToSave.userID = uid;
                dataToSave.userName = finalUsername;
                Debug.Log($"[Auth] Converted Guest to User. Preserving XP: {dataToSave.xp}");
            }
            else
            {
                // No guest data, create fresh profile
                dataToSave = new PlayerData 
                { 
                    userID = uid,
                    userName = finalUsername 
                };
                Debug.Log($"[Auth] Created fresh user: {finalUsername}");
            }

            // Now sync this merged data to the cloud
            if (CloudSyncManager.Instance != null)
            {
                CloudSyncManager.Instance.SyncProgress(dataToSave); 
            }
        }

        private void LoadWorldMap()
        {
            if (CloudSyncManager.Instance != null && _auth.CurrentUser != null)
            {
                // 1. Tell CloudSyncManager to fetch the user data
                CloudSyncManager.Instance.InitializeAsUser(_auth.CurrentUser.UserId);
                
                // 2. Start a Coroutine to wait for the data to arrive before switching scenes
                StartCoroutine(WaitForDataAndRedirect());
            }
        }

        private IEnumerator WaitForDataAndRedirect()
        {
            // Wait until CloudSyncManager has finished fetching the profile
            yield return new WaitUntil(() => CloudSyncManager.Instance.CurrentData != null);

            PlayerData data = CloudSyncManager.Instance.CurrentData;

            // Check the flag defined in PlayerData.cs
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
        }

        private void UpdateFeedback(string msg) { if (feedbackText) feedbackText.text = msg; }
    }
}