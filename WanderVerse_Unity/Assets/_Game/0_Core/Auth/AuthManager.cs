using UnityEngine;
using TMPro;                
using Firebase.Auth;
using Firebase.Firestore;   
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

        void Start()
        {
            _auth = FirebaseAuth.DefaultInstance;
            _db = FirebaseFirestore.DefaultInstance; 

            if(feedbackText != null) feedbackText.text = "";

            if (signUpUsernameInput != null) 
            {
                signUpUsernameInput.readOnly = true; 
            }

            // Auto logs in a user
            /*if (_auth.CurrentUser != null)
            {
                Debug.Log($"[Auth] Welcome back, {_auth.CurrentUser.UserId}");
                LoadWorldMap();
            }*/
        }


        public void OnGenerateUsernameButton()
        {
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

            string randomAdj = adjectives[UnityEngine.Random.Range(0, adjectives.Length)];
            string randomNoun = nouns[UnityEngine.Random.Range(0, nouns.Length)];

            signUpUsernameInput.text = $"{randomAdj}{randomNoun}";
        }
        
        public void OnSignUpButton() 
        {
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

        public void OnGuestLoginButton() => StartCoroutine(AuthRoutine(
            () => _auth.SignInAnonymouslyAsync(), true));


        private IEnumerator SignUpRoutine(string username, string email, string password)
        {
            if (_isWorking) yield break;
            _isWorking = true;
            UpdateFeedback("Checking availability...");

            // Checks if Username is taken in Firestore
            var checkTask = _db.Collection("usernames").Document(username).GetSnapshotAsync();
            yield return new WaitUntil(() => checkTask.IsCompleted);

            if (checkTask.Result.Exists)
            {
                UpdateFeedback($"'{username}' is already taken! Generate again.");
                _isWorking = false;
                yield break;
            }

            // Creates Firebase Auth User
            UpdateFeedback("Creating Account...");
            var authTask = _auth.CreateUserWithEmailAndPasswordAsync(email, password);
            yield return new WaitUntil(() => authTask.IsCompleted);

            if (authTask.Exception != null) 
            {
                Debug.LogError($"Auth Error: {authTask.Exception.InnerException?.Message}");
                UpdateFeedback($"Error: {authTask.Exception.InnerException?.Message}");
                _isWorking = false;
            } 
            else 
            {
                string uid = authTask.Result.User.UserId;
                
                // Saves username --> maps username with email
                yield return StartCoroutine(RegisterUsernameInDB(username, email, uid));

                InitializeNewUserData(uid);

                UpdateFeedback("Success! Redirecting to Login...");
                yield return new WaitForSeconds(2.0f);

                _auth.SignOut(); // Stops auto login

                if (panelSignUp != null) panelSignUp.SetActive(false);
                if (panelSignIn != null) panelSignIn.SetActive(true);

                if (loginEmailInput) loginEmailInput.text = ""; 
                if (loginPasswordInput) loginPasswordInput.text = "";
                UpdateFeedback("Account created. Please log in.");
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

            PlayerData newData = new PlayerData 
            { 
                userID = uid,
                userName = finalUsername 
            };

            // TODO: @Senmith - Uncomment the below line when LocalDataManager is ready
            // LocalDataManager.Save(newData); 
            
            if (CloudSyncManager.Instance != null)
            {
                CloudSyncManager.Instance.SyncProgress(newData); 
            }

            Debug.Log($"[Auth] Created new user: {finalUsername}");
        }

        private void LoadWorldMap()
        {
            if (CloudSyncManager.Instance != null && _auth.CurrentUser != null)
                CloudSyncManager.Instance.InitializeData(_auth.CurrentUser.UserId);
            
            SceneManager.LoadScene("Scene_WorldMap");
        }

        private void UpdateFeedback(string msg) { if (feedbackText) feedbackText.text = msg; }
    }
}