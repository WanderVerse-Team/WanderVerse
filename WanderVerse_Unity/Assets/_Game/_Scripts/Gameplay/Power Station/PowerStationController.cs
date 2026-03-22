using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using WanderVerse.Framework.Data;

/// <summary>
/// Main controller for the Power Station mini-game (Addition 1).
/// Extends BaseLevelController using GameType.Addition1.
///
/// Each level has multiple TURNS. Every turn generates a new random target power,
/// builds the correct addends, creates distractor batteries, and lets the player
/// drag batteries into a vertical-addition grid.
///
/// The number of turns is configured via LevelData.totalTurns (editable in the Inspector / backend).
/// </summary>
public class PowerStationController : BaseLevelController
{
    // ─── Tell the framework our type ───
    protected override GameType SupportedGameType => GameType.Addition1;

    // ═══════════════════════════════════════════
    //  INSPECTOR REFERENCES
    // ═══════════════════════════════════════════

    [Header("--- Power Station UI ---")]
    [Tooltip("TMP text showing the full target power number (e.g. '79')")]
    public TextMeshProUGUI targetPowerText;

    [Tooltip("Individual TMP texts for each digit of the target, left-to-right (index 0 = highest place)")]
    public TextMeshProUGUI[] targetDigitTexts;

    [Tooltip("Parent transform that holds the battery tray (available batteries)")]
    public Transform batteryTray;

    [Tooltip("Maximum number of batteries visible in the tray at one time")]
    public int maxVisibleBatteries = 6;

    [Tooltip("Number of columns in the battery tray grid")]
    public int trayColumns = 3;

    [Tooltip("Number of rows in the battery tray grid")]
    public int trayRows = 2;

    [Tooltip("Horizontal spacing between batteries in tray")]
    public float traySpacingX = 12f;

    [Tooltip("Vertical spacing between battery rows in tray")]
    public float traySpacingY = 8f;

    [Tooltip("Prefab for a single battery UI element (must have BatteryIdentity + BatteryDragDrop + CanvasGroup)")]
    public GameObject batteryPrefab;

    [Tooltip("All sockets in the grid. Order: row-major (R0C0, R0C1, R1C0, R1C1, …)")]
    public BatterySocket[] sockets;

    [Header("--- Column Sum Display ---")]
    [Tooltip("TMP texts showing the live sum of each column (index 0 = leftmost/highest place). Same order as targetDigitTexts.")]
    public TextMeshProUGUI[] columnSumTexts;

    [Tooltip("TMP texts for carry indicators above each column (index 0 = leftmost). Show '+1' when a carry occurs from the column to its right.")]
    public TextMeshProUGUI[] carryTexts;

    [Tooltip("Optional global carry indicator object (e.g., a '+1' icon named Carry). Will be shown when any carry exists.")]
    public GameObject carryIndicatorObject;

    [Tooltip("Images representing the energy flow lines inside each column pipe. Used to tint green/red as column sums change.")]
    public Image[] columnPipeImages;

    [Header("--- Turn Progress UI ---")]
    [Tooltip("TMP text showing current turn / total turns, e.g. 'Round 1 / 3'")]
    public TextMeshProUGUI turnProgressText;

    [Header("--- Machine Visuals ---")]
    public Image machineImage;
    public Animator machineAnimator;
    public Sprite machineIdle;
    public Sprite machineUnderload;
    public Sprite machineOverload;
    public Sprite machineVictory;
    public string idleStateName = "machine_idle";
    public string overloadStateName = "machine_overload";
    public string underloadStateName = "machine_underload";
    public string victoryStateName = "machine_victory";
    public string overloadTriggerName = "Overload";
    public string underloadTriggerName = "Underload";
    public string victoryTriggerName = "Victory";

    [Header("--- Feedback Panels ---")]
    public RectTransform correctPanel;
    public RectTransform overloadPanel;
    public RectTransform underloadPanel;

    [Header("--- Panel Animation ---")]
    public float hiddenY = 800f;
    public float visibleY = 0f;
    public float waitTime = 2.0f;
    public float targetX = 0f;

    [Header("--- Audio ---")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip overloadSound;
    public AudioClip underloadSound;

    [Header("--- Victory ---")]
    public GameObject victoryPanel;
    public AudioSource victoryAudioSource;
    public AudioClip victoryChime;
    public ParticleSystem confetti;
    public ParticleSystem sparks;

    [Header("--- Victory Timing ---")]
    [Tooltip("Delay before ending the level so machine_victory animation can play before WinScreen appears.")]
    public float victoryAnimationDuration = 1.5f;

    [Header("--- Submit ---")]
    public Button submitButton;

    [Header("--- Orientation (Power Station Only) ---")]
    [Tooltip("Force landscape while Power Station scene is active.")]
    public bool forceLandscapeOnEnter = true;

    [Tooltip("Restore portrait when leaving Power Station scene.")]
    public bool restorePortraitOnExit = true;

    // ═══════════════════════════════════════════
    //  INTERNAL STATE
    // ═══════════════════════════════════════════
    private int currentTurn = 0;
    private int totalTurns;
    private int currentTargetPower;
    private int numColumns;
    private int numRows;
    private bool isProcessingResult = false;
    private bool useManualTrayGridLayout = false;
    private bool isRefreshingTray = false;
    private Dictionary<int, int> requiredDigitCounts = new Dictionary<int, int>();
    private int[,] solutionDigits;
    private Coroutine forceIdleRoutine;

    private void EnsureMachineAnimatorAssigned()
    {
        if (machineAnimator != null) return;

        if (machineImage != null)
            machineAnimator = machineImage.GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (forceLandscapeOnEnter)
            ApplyLandscapeOrientation();
    }

    private void OnDisable()
    {
        if (restorePortraitOnExit)
            ApplyPortraitOrientation();
    }

    private void ApplyLandscapeOrientation()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.orientation = ScreenOrientation.LandscapeLeft;
    }

    private void ApplyPortraitOrientation()
    {
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.orientation = ScreenOrientation.Portrait;
    }

    private bool TryPlayMachineState(string stateName)
    {
        if (machineAnimator == null || string.IsNullOrEmpty(stateName)) return false;

        int stateHash = Animator.StringToHash(stateName);
        if (!machineAnimator.HasState(0, stateHash)) return false;

        machineAnimator.Play(stateName, 0, 0f);
        return true;
    }

    private bool TryPlayFirstAvailableMachineState(string[] stateNames)
    {
        if (stateNames == null || stateNames.Length == 0) return false;

        for (int i = 0; i < stateNames.Length; i++)
        {
            if (TryPlayMachineState(stateNames[i]))
                return true;
        }

        return false;
    }

    private void SetMachineIdleVisual()
    {
        EnsureMachineAnimatorAssigned();

        // Immediate visual fallback so the machine does not remain on previous state's last frame.
        if (machineImage != null && machineIdle != null)
            machineImage.sprite = machineIdle;

        if (machineAnimator != null)
        {
            if (HasAnimatorTrigger(overloadTriggerName))
                machineAnimator.ResetTrigger(overloadTriggerName);

            if (HasAnimatorTrigger(underloadTriggerName))
                machineAnimator.ResetTrigger(underloadTriggerName);

            if (HasAnimatorTrigger(victoryTriggerName))
                machineAnimator.ResetTrigger(victoryTriggerName);

            string[] idleCandidates = { idleStateName, "machine_idle", "Machine_Idle", "Idle" };

            if (TryPlayFirstAvailableMachineState(idleCandidates))
            {
                return;
            }

            // Rebind and try again once (helps after one-shot states/transitions).
            machineAnimator.Rebind();
            machineAnimator.Update(0f);

            if (TryPlayFirstAvailableMachineState(idleCandidates))
                return;

            Debug.LogWarning($"[PowerStation] Animator state '{idleStateName}' not found. Falling back to idle sprite.");
        }
    }

    private void SetMachineOverloadVisual()
    {
        EnsureMachineAnimatorAssigned();

        if (machineAnimator != null)
        {
            if (TryPlayMachineState(overloadStateName))
                return;

            if (HasAnimatorTrigger(overloadTriggerName))
            {
                machineAnimator.SetTrigger(overloadTriggerName);
                return;
            }

            Debug.LogWarning($"[PowerStation] Animator trigger '{overloadTriggerName}' not found. Falling back to overload sprite.");
        }

        if (machineImage != null && machineOverload != null)
            machineImage.sprite = machineOverload;
    }

    private void SetMachineUnderloadVisual()
    {
        EnsureMachineAnimatorAssigned();

        if (machineAnimator != null)
        {
            if (TryPlayMachineState(underloadStateName))
                return;

            if (HasAnimatorTrigger(underloadTriggerName))
            {
                machineAnimator.SetTrigger(underloadTriggerName);
                return;
            }

            Debug.LogWarning($"[PowerStation] Animator trigger '{underloadTriggerName}' not found. Falling back to underload sprite.");
        }

        if (machineImage != null && machineUnderload != null)
            machineImage.sprite = machineUnderload;
    }

    private void SetMachineVictoryVisual()
    {
        EnsureMachineAnimatorAssigned();

        if (machineAnimator != null)
        {
            if (TryPlayMachineState(victoryStateName))
                return;

            if (HasAnimatorTrigger(victoryTriggerName))
            {
                machineAnimator.SetTrigger(victoryTriggerName);
                return;
            }

            Debug.LogWarning($"[PowerStation] Animator trigger '{victoryTriggerName}' not found. Falling back to victory sprite.");
        }

        if (machineImage != null && machineVictory != null)
            machineImage.sprite = machineVictory;
    }

    private bool HasAnimatorTrigger(string triggerName)
    {
        if (machineAnimator == null || string.IsNullOrEmpty(triggerName)) return false;

        AnimatorControllerParameter[] parameters = machineAnimator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            AnimatorControllerParameter parameter = parameters[i];
            if (parameter.type == AnimatorControllerParameterType.Trigger && parameter.name == triggerName)
                return true;
        }

        return false;
    }

    private void ForceMachineIdleForTurnStart()
    {
        SetMachineIdleVisual();

        if (forceIdleRoutine != null)
            StopCoroutine(forceIdleRoutine);

        forceIdleRoutine = StartCoroutine(ForceIdleNextFrame());
    }

    private IEnumerator ForceIdleNextFrame()
    {
        yield return null;
        SetMachineIdleVisual();

        yield return new WaitForEndOfFrame();
        SetMachineIdleVisual();

        forceIdleRoutine = null;
    }

    private void SetBoardVisualsVisible(bool visible)
    {
        if (columnSumTexts != null)
        {
            for (int i = 0; i < columnSumTexts.Length; i++)
            {
                if (columnSumTexts[i] != null)
                    columnSumTexts[i].gameObject.SetActive(visible);
            }
        }

        if (!visible)
        {
            if (carryTexts != null)
            {
                for (int i = 0; i < carryTexts.Length; i++)
                {
                    if (carryTexts[i] != null)
                        carryTexts[i].gameObject.SetActive(false);
                }
            }

            if (carryIndicatorObject != null)
                carryIndicatorObject.SetActive(false);
        }

        if (sockets != null)
        {
            for (int i = 0; i < sockets.Length; i++)
            {
                BatterySocket socket = sockets[i];
                if (socket == null || socket.currentBattery == null) continue;
                socket.currentBattery.gameObject.SetActive(visible);
            }
        }
    }

    // ═══════════════════════════════════════════
    //  LIFECYCLE
    // ═══════════════════════════════════════════

    protected override void Start()
    {
        base.Start(); // BaseLevelController: validate, load data, InitializeLevel, etc.

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (confetti != null) confetti.Stop();
        if (sparks != null) sparks.Stop();
    }

    /// <summary>
    /// Called once by BaseLevelController.Start() after data is loaded.
    /// Sets up the first turn.
    /// </summary>
    protected override void InitializeLevel()
    {
        EnsureLevelIdConfiguredForPowerStation();

        // Read configurable values from the ScriptableObject
        totalTurns = levelData.totalTurns;
        numRows    = levelData.batteryRows;
        currentTurn = 0;

        ValidateTraySettings();
        EnsureSocketsInitialized();
        EnsureColumnSumTextsInitialized();
        EnsureCarryIndicatorInitialized();
        ConfigureBatteryTrayLayout();

        // Wire submit button
        if (submitButton != null)
        {
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(OnSubmitPressed);
        }

        // Start the first turn
        StartNextTurn();
    }

    private void EnsureLevelIdConfiguredForPowerStation()
    {
        if (levelData == null) return;

        if (string.IsNullOrWhiteSpace(levelData.levelID) || levelData.levelID == CourseCatalog.NONE)
        {
            levelData.levelID = CourseCatalog.L03_POW_1;
            Debug.LogWarning("[PowerStation] LevelData levelID was unassigned. Defaulted to L03_POW_1 so completion and WinScreen flow can proceed.");
        }
    }

    private void EnsureCarryIndicatorInitialized()
    {
        if (carryIndicatorObject == null)
        {
            GameObject found = GameObject.Find("Carry");
            if (found != null)
                carryIndicatorObject = found;
        }

        // Fallback: find inactive Carry object in the loaded scene
        if (carryIndicatorObject == null)
        {
            GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            Scene activeScene = SceneManager.GetActiveScene();

            for (int i = 0; i < allObjects.Length; i++)
            {
                GameObject obj = allObjects[i];
                if (obj == null) continue;
                if (!obj.scene.IsValid() || obj.scene != activeScene) continue;

                if (string.Equals(obj.name, "Carry", System.StringComparison.OrdinalIgnoreCase))
                {
                    carryIndicatorObject = obj;
                    break;
                }
            }
        }

        if (carryIndicatorObject != null)
            carryIndicatorObject.SetActive(false);
        else
            Debug.LogWarning("[PowerStation] Carry indicator object not found. Assign Carry object in inspector or name it 'Carry'.");
    }

    private void EnsureColumnSumTextsInitialized()
    {
        if (numColumns <= 0) return;

        if (columnSumTexts == null || columnSumTexts.Length < numColumns)
            columnSumTexts = new TextMeshProUGUI[numColumns];

        bool hasMissing = false;
        for (int i = 0; i < numColumns; i++)
        {
            if (columnSumTexts[i] == null)
            {
                hasMissing = true;
                break;
            }
        }

        if (!hasMissing) return;

        TextMeshProUGUI[] allTexts = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        for (int i = 0; i < numColumns; i++)
        {
            if (columnSumTexts[i] != null) continue;

            string expectedName = $"ColumnSum_{i}";
            for (int t = 0; t < allTexts.Length; t++)
            {
                TextMeshProUGUI text = allTexts[t];
                if (text == null) continue;

                if (string.Equals(text.name, expectedName, System.StringComparison.OrdinalIgnoreCase))
                {
                    columnSumTexts[i] = text;
                    if (!text.gameObject.activeSelf)
                        text.gameObject.SetActive(true);
                    break;
                }
            }
        }

        for (int i = 0; i < numColumns; i++)
        {
            if (columnSumTexts[i] == null)
                Debug.LogWarning($"[PowerStation] Missing ColumnSum_{i} reference. Assign it in inspector or keep exact name for auto-wire.");
        }
    }

    private void EnsureSocketsInitialized()
    {
        int expectedSocketCount = Mathf.Max(1, numRows) * Mathf.Max(1, numColumns);
        bool hasSerializedSockets = sockets != null && sockets.Length > 0;
        if (hasSerializedSockets)
        {
            bool allValid = true;
            for (int i = 0; i < sockets.Length; i++)
            {
                if (sockets[i] == null)
                {
                    allValid = false;
                    break;
                }
            }

            if (allValid && sockets.Length >= expectedSocketCount) return;
        }

        List<BatterySocket> discovered = new List<BatterySocket>();

        // 1) Existing BatterySocket components in scene
        BatterySocket[] existing = FindObjectsByType<BatterySocket>(FindObjectsSortMode.None);
        if (existing != null && existing.Length > 0)
            discovered.AddRange(existing);

        // 2) Enforce BatterySocket on objects named Socket_Rx_Cy
        RectTransform[] rects = FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
        foreach (RectTransform rect in rects)
        {
            if (rect == null) continue;
            if (!TryParseSocketName(rect.name, out int row, out int column)) continue;

            BatterySocket socket = rect.GetComponent<BatterySocket>();
            if (socket == null)
                socket = rect.gameObject.AddComponent<BatterySocket>();

            socket.row = row;
            socket.column = column;

            if (!discovered.Contains(socket))
                discovered.Add(socket);
        }

        // Normalize socket coordinates from object names when available (authoritative)
        for (int i = 0; i < discovered.Count; i++)
        {
            BatterySocket socket = discovered[i];
            if (socket == null) continue;

            if (TryParseSocketName(socket.name, out int parsedRow, out int parsedColumn))
            {
                socket.row = parsedRow;
                socket.column = parsedColumn;
            }
        }

        discovered.Sort((a, b) =>
        {
            int rowCompare = a.row.CompareTo(b.row);
            return rowCompare != 0 ? rowCompare : a.column.CompareTo(b.column);
        });

        sockets = discovered.ToArray();

        if (sockets.Length == 0)
            Debug.LogError("[PowerStation] No sockets found. Add objects named like Socket_R0_C0, Socket_R0_C1, Socket_R1_C0, Socket_R1_C1.");
        else
            Debug.Log($"[PowerStation] Auto-wired {sockets.Length} sockets.");

        for (int i = 0; i < sockets.Length; i++)
        {
            if (sockets[i] == null) continue;
            Debug.Log($"[PowerStation] Socket map: {sockets[i].name} => row {sockets[i].row}, col {sockets[i].column}");
        }
    }

    private bool TryParseSocketName(string objectName, out int row, out int column)
    {
        row = -1;
        column = -1;

        if (string.IsNullOrEmpty(objectName)) return false;
        if (!objectName.StartsWith("Socket_R", System.StringComparison.OrdinalIgnoreCase)) return false;

        string[] parts = objectName.Split('_');
        if (parts.Length < 3) return false;

        string rowPart = parts[1]; // e.g. R0
        string colPart = parts[2]; // e.g. C1

        if (rowPart.Length < 2 || colPart.Length < 2) return false;
        if (!int.TryParse(rowPart.Substring(1), out row)) return false;
        if (!int.TryParse(colPart.Substring(1), out column)) return false;

        return true;
    }

    // ═══════════════════════════════════════════
    //  TURN MANAGEMENT
    // ═══════════════════════════════════════════

    /// <summary>
    /// Generates a new random target, builds correct addends and distractors,
    /// refreshes the UI, and lets the player solve.
    /// </summary>
    private void StartNextTurn()
    {
        currentTurn++;

        SetBoardVisualsVisible(true);

        // 1. Generate a random target power within the configured range
        currentTargetPower = Random.Range(levelData.minTargetPower, levelData.maxTargetPower + 1);

        // 2. Figure out how many columns (digits) the target has
        numColumns = currentTargetPower.ToString().Length;

        // Ensure ColumnSum_0/1 references are valid for current digit count
        EnsureColumnSumTextsInitialized();

        // 3. Update UI
        UpdateTargetDisplay();
        UpdateTurnProgress();

        // 4. Generate batteries (correct addends + distractors)
        List<int> batteries = GenerateBatteryPool();

        // 5. Spawn battery UI elements in the tray
        SpawnBatteries(batteries);

        // 6. Clear all sockets
        ClearAllSockets();

        // 7. Reset column sum display
        ResetColumnSumDisplay();

        // 8. Reset machine visual
        ForceMachineIdleForTurnStart();

        Debug.Log($"[PowerStation] Turn {currentTurn}/{totalTurns} — Target: {currentTargetPower} | Columns: {numColumns} | Rows: {numRows}");
    }

    // ═══════════════════════════════════════════
    //  TARGET POWER GENERATION & DISPLAY
    // ═══════════════════════════════════════════

    private void UpdateTargetDisplay()
    {
        // Full number text
        if (targetPowerText != null)
            targetPowerText.text = currentTargetPower.ToString();

        // Per-digit texts (for the column-aligned display under the machine)
        string targetStr = currentTargetPower.ToString();
        for (int i = 0; i < targetDigitTexts.Length; i++)
        {
            if (i < targetStr.Length)
                targetDigitTexts[i].text = targetStr[i].ToString();
            else
                targetDigitTexts[i].text = "";
        }
    }

    private void UpdateTurnProgress()
    {
        if (turnProgressText != null)
            turnProgressText.text = $"Round {currentTurn} / {totalTurns}";
    }

    // ═══════════════════════════════════════════
    //  BATTERY POOL GENERATION
    // ═══════════════════════════════════════════

    /// <summary>
    /// Generates a list of battery digit values that:
    ///   a) Contains one valid set of addend digits that sum to the target.
    ///   b) Adds extra random distractor digits to increase difficulty.
    /// </summary>
    private List<int> GenerateBatteryPool()
    {
        ValidateTraySettings();
        int trayCapacity = GetTrayCapacity();

        List<int> lastCandidate = null;
        const int maxGenerationAttempts = 20;

        for (int attempt = 0; attempt < maxGenerationAttempts; attempt++)
        {
            List<int> pool = new List<int>();
            requiredDigitCounts.Clear();

            // --- Step A: Build valid addends whose sum == currentTargetPower ---
            int[] addends = GenerateAddends(currentTargetPower, numRows);
            solutionDigits = new int[numRows, numColumns];

            for (int r = 0; r < addends.Length; r++)
            {
                string addendStr = addends[r].ToString().PadLeft(numColumns, '0');
                for (int c = 0; c < addendStr.Length; c++)
                {
                    int digit = int.Parse(addendStr[c].ToString());
                    pool.Add(digit);
                    solutionDigits[r, c] = digit;

                    if (!requiredDigitCounts.ContainsKey(digit))
                        requiredDigitCounts[digit] = 0;
                    requiredDigitCounts[digit]++;
                }
            }

            // --- Step B: Add distractor batteries ---
            int maxDistractorsByCapacity = Mathf.Max(0, trayCapacity - pool.Count);
            int distractorCount = Mathf.Min(levelData.extraDistractorBatteries, maxDistractorsByCapacity);
            for (int i = 0; i < distractorCount; i++)
                pool.Add(Random.Range(0, 10));

            while (pool.Count < trayCapacity)
                pool.Add(Random.Range(0, 10));

            ShuffleList(pool);

            if (pool.Count > trayCapacity)
                pool = pool.GetRange(0, trayCapacity);

            lastCandidate = pool;

            if (IsPoolSolvableForTarget(pool))
                return pool;
        }

        Debug.LogWarning("[PowerStation] Could not generate a solvable tray in time. Using deterministic fallback pool.");

        List<int> fallbackPool = new List<int>();
        requiredDigitCounts.Clear();

        if (solutionDigits != null)
        {
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < numColumns; c++)
                {
                    int digit = solutionDigits[r, c];
                    fallbackPool.Add(digit);
                    if (!requiredDigitCounts.ContainsKey(digit))
                        requiredDigitCounts[digit] = 0;
                    requiredDigitCounts[digit]++;
                }
            }
        }

        while (fallbackPool.Count < trayCapacity)
            fallbackPool.Add(Random.Range(0, 10));

        if (fallbackPool.Count > trayCapacity)
            fallbackPool = fallbackPool.GetRange(0, trayCapacity);

        if (!IsPoolSolvableForTarget(fallbackPool) && lastCandidate != null)
            return lastCandidate;

        ShuffleList(fallbackPool);
        return fallbackPool;
    }

    private bool IsPoolSolvableForTarget(List<int> pool)
    {
        if (pool == null) return false;

        int requiredCells = numRows * numColumns;
        if (requiredCells <= 0) return false;
        if (pool.Count < requiredCells) return false;

        int[] counts = new int[10];
        for (int i = 0; i < pool.Count; i++)
        {
            int digit = Mathf.Clamp(pool[i], 0, 9);
            counts[digit]++;
        }

        int[] placeValues = new int[numColumns];
        for (int c = 0; c < numColumns; c++)
            placeValues[c] = (int)Mathf.Pow(10, numColumns - 1 - c);

        int[] rowAddends = new int[numRows];
        return TryBuildRowsForTarget(0, counts, rowAddends, placeValues);
    }

    private bool TryBuildRowsForTarget(int cellIndex, int[] counts, int[] rowAddends, int[] placeValues)
    {
        int totalCells = numRows * numColumns;
        if (cellIndex >= totalCells)
        {
            int total = 0;
            for (int r = 0; r < rowAddends.Length; r++)
                total += rowAddends[r];

            return total == currentTargetPower;
        }

        int row = cellIndex / numColumns;
        int column = cellIndex % numColumns;
        int placeValue = placeValues[column];

        for (int digit = 0; digit <= 9; digit++)
        {
            if (counts[digit] <= 0) continue;

            counts[digit]--;
            rowAddends[row] += digit * placeValue;

            if (TryBuildRowsForTarget(cellIndex + 1, counts, rowAddends, placeValues))
                return true;

            rowAddends[row] -= digit * placeValue;
            counts[digit]++;
        }

        return false;
    }

    private void ConfigureBatteryTrayLayout()
    {
        ValidateTraySettings();

        if (batteryTray == null)
        {
            Debug.LogWarning("[PowerStation] Battery Tray is not assigned.");
            return;
        }

        GameObject trayObject = batteryTray.gameObject;
        if (trayObject == null)
        {
            Debug.LogWarning("[PowerStation] Battery Tray GameObject is missing.");
            return;
        }

        useManualTrayGridLayout = false;

        HorizontalLayoutGroup horizontal = trayObject.GetComponent<HorizontalLayoutGroup>();
        if (horizontal != null) horizontal.enabled = false;

        ContentSizeFitter fitter = trayObject.GetComponent<ContentSizeFitter>();
        if (fitter != null) fitter.enabled = false;

        GridLayoutGroup grid = trayObject.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            LayoutGroup anyLayout = trayObject.GetComponent<LayoutGroup>();
            if (anyLayout != null && !(anyLayout is GridLayoutGroup))
            {
                useManualTrayGridLayout = true;
                Debug.LogWarning("[PowerStation] Tray already has a non-grid LayoutGroup. Using manual 2x3 layout fallback.");
                return;
            }

            grid = trayObject.AddComponent<GridLayoutGroup>();
            if (grid == null)
            {
                useManualTrayGridLayout = true;
                Debug.LogWarning("[PowerStation] Could not add GridLayoutGroup. Using manual 2x3 layout fallback.");
                return;
            }
        }

        RectTransform batteryRect = batteryPrefab != null ? batteryPrefab.GetComponent<RectTransform>() : null;
        Vector2 cellSize = batteryRect != null && batteryRect.sizeDelta != Vector2.zero
            ? batteryRect.sizeDelta
            : new Vector2(80f, 120f);

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = Mathf.Max(1, trayColumns);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.cellSize = cellSize;
        grid.spacing = new Vector2(traySpacingX, traySpacingY);
    }

    /// <summary>
    /// Generates an array of random positive integers that sum to exactly 'target'.
    /// Each addend will have at most 'numColumns' digits.
    /// </summary>
    private int[] GenerateAddends(int target, int rows)
    {
        int[] addends = new int[rows];
        int remaining = target;

        for (int i = 0; i < rows - 1; i++)
        {
            // Each addend must be at least 1 and leave room for the remaining rows
            int minVal = 1;
            int maxVal = remaining - (rows - 1 - i); // Leave at least 1 for each remaining row

            // Clamp to valid digit count (e.g., max 99 for 2-digit)
            int maxForDigits = (int)Mathf.Pow(10, numColumns) - 1;
            maxVal = Mathf.Min(maxVal, maxForDigits);
            maxVal = Mathf.Max(maxVal, minVal);

            addends[i] = Random.Range(minVal, maxVal + 1);
            remaining -= addends[i];
        }

        // Last addend gets whatever is left
        addends[rows - 1] = remaining;

        return addends;
    }

    // ═══════════════════════════════════════════
    //  SPAWN & CLEAR
    // ═══════════════════════════════════════════

    private void SpawnBatteries(List<int> values)
    {
        ValidateTraySettings();

        if (batteryTray == null)
        {
            Debug.LogWarning("[PowerStation] Cannot spawn batteries: Battery Tray is not assigned.");
            return;
        }

        if (batteryPrefab == null)
        {
            Debug.LogWarning("[PowerStation] Cannot spawn batteries: Battery Prefab is not assigned.");
            return;
        }

        // Clear old tray batteries immediately from layout flow (destroy happens end-of-frame)
        List<Transform> oldChildren = new List<Transform>();
        for (int i = 0; i < batteryTray.childCount; i++)
            oldChildren.Add(batteryTray.GetChild(i));

        foreach (Transform child in oldChildren)
        {
            child.SetParent(null, false);
            Destroy(child.gameObject);
        }

        // Instantiate one battery UI element per value
        int maxToSpawn = Mathf.Min(values.Count, GetTrayCapacity());
        for (int i = 0; i < maxToSpawn; i++)
        {
            int val = values[i];
            GameObject go = Instantiate(batteryPrefab, batteryTray);
            BatteryIdentity id = go.GetComponent<BatteryIdentity>();
            if (id != null)
                id.Setup(val);
        }

        if (useManualTrayGridLayout)
            ApplyManualTrayGridLayout();
    }

    private void ApplyManualTrayGridLayout()
    {
        if (batteryTray == null) return;

        RectTransform batteryRect = batteryPrefab != null ? batteryPrefab.GetComponent<RectTransform>() : null;
        Vector2 cellSize = batteryRect != null && batteryRect.sizeDelta != Vector2.zero
            ? batteryRect.sizeDelta
            : new Vector2(80f, 120f);

        int columns = Mathf.Max(1, trayColumns);
        int rows = Mathf.Max(1, trayRows);
        int maxCells = columns * rows;

        float totalWidth = columns * cellSize.x + (columns - 1) * traySpacingX;
        float totalHeight = rows * cellSize.y + (rows - 1) * traySpacingY;

        int childCount = Mathf.Min(batteryTray.childCount, maxCells);
        for (int i = 0; i < childCount; i++)
        {
            RectTransform child = batteryTray.GetChild(i) as RectTransform;
            if (child == null) continue;

            int row = i / columns;
            int column = i % columns;

            float x = -totalWidth * 0.5f + cellSize.x * 0.5f + column * (cellSize.x + traySpacingX);
            float y = totalHeight * 0.5f - cellSize.y * 0.5f - row * (cellSize.y + traySpacingY);

            child.anchorMin = new Vector2(0.5f, 0.5f);
            child.anchorMax = new Vector2(0.5f, 0.5f);
            child.pivot = new Vector2(0.5f, 0.5f);
            child.anchoredPosition = new Vector2(x, y);
            child.sizeDelta = cellSize;
            child.localScale = Vector3.one;
        }
    }

    private void ClearAllSockets()
    {
        foreach (var socket in sockets)
        {
            // Destroy the battery in the socket rather than returning it
            if (socket.currentBattery != null)
            {
                Destroy(socket.currentBattery.gameObject);
                socket.currentBattery = null;
            }
        }
    }

    // ═══════════════════════════════════════════
    //  SOCKET EVENT
    // ═══════════════════════════════════════════

    /// <summary>Called by BatterySocket whenever a battery is placed or removed.</summary>
    public void OnBatteryPlaced()
    {
        UpdateColumnSums();
        EnsureTraySolvableOrRefresh();
        Debug.Log("[PowerStation] Battery placed — column sums updated.");
    }

    /// <summary>Called after a battery successfully snaps into a socket.</summary>
    public void OnBatterySnappedToSocket()
    {
        RefillTrayAfterSnap();
        OnBatteryPlaced();
    }

    /// <summary>Called when a battery is dropped into the dustbin.</summary>
    public void OnBatteryDiscarded(bool refillTray = true)
    {
        if (refillTray)
            RefillTrayToCapacity();

        OnBatteryPlaced();
    }

    private void RefillTrayAfterSnap()
    {
        ValidateTraySettings();

        if (batteryTray == null || batteryPrefab == null) return;

        int trayCapacity = GetTrayCapacity();
        if (batteryTray.childCount >= trayCapacity) return;

        GameObject go = Instantiate(batteryPrefab, batteryTray);
        BatteryIdentity id = go.GetComponent<BatteryIdentity>();
        if (id != null)
            id.Setup(GetRefillDigitValue());

        if (useManualTrayGridLayout)
            ApplyManualTrayGridLayout();
    }

    private void RefillTrayToCapacity()
    {
        if (batteryTray == null || batteryPrefab == null) return;

        int trayCapacity = GetTrayCapacity();
        while (batteryTray.childCount < trayCapacity)
        {
            GameObject go = Instantiate(batteryPrefab, batteryTray);
            BatteryIdentity id = go.GetComponent<BatteryIdentity>();
            if (id != null)
                id.Setup(GetRefillDigitValue());
        }

        if (useManualTrayGridLayout)
            ApplyManualTrayGridLayout();
    }

    private void EnsureTraySolvableOrRefresh()
    {
        if (isRefreshingTray) return;
        if (solutionDigits == null || sockets == null || batteryTray == null || batteryPrefab == null) return;

        if (IsTraySolvableForRemainingSockets()) return;

        RefreshTrayForRemainingSockets();
    }

    private bool IsTraySolvableForRemainingSockets()
    {
        Dictionary<int, int> trayCounts = new Dictionary<int, int>();
        foreach (Transform child in batteryTray)
        {
            BatteryIdentity battery = child.GetComponent<BatteryIdentity>();
            if (battery == null) continue;

            int digit = battery.digitValue;
            if (!trayCounts.ContainsKey(digit)) trayCounts[digit] = 0;
            trayCounts[digit]++;
        }

        return TryFindCompletionForCurrentBoard(trayCounts, out _);
    }

    private void RefreshTrayForRemainingSockets()
    {
        if (batteryTray == null || batteryPrefab == null) return;

        isRefreshingTray = true;

        List<int> refreshed = new List<int>();
        int trayCapacity = GetTrayCapacity();

        // Add digits that solve the currently remaining empty sockets.
        if (TryFindCompletionForCurrentBoard(null, out List<int> requiredForRemaining))
            refreshed.AddRange(requiredForRemaining);

        // Fill remaining tray slots with random distractors.
        while (refreshed.Count < trayCapacity)
            refreshed.Add(Random.Range(0, 10));

        if (refreshed.Count > trayCapacity)
            refreshed = refreshed.GetRange(0, trayCapacity);

        ShuffleList(refreshed);
        SpawnBatteries(refreshed);

        isRefreshingTray = false;

        Debug.Log("[PowerStation] Tray refreshed to keep target reachable.");
    }

    private bool TryFindCompletionForCurrentBoard(Dictionary<int, int> availableDigits, out List<int> requiredDigits)
    {
        requiredDigits = new List<int>();

        if (numRows <= 0 || numColumns <= 0) return false;

        int[] rowValues = new int[numRows];
        List<int> emptyRows = new List<int>();
        List<int> emptyPlaceValues = new List<int>();

        for (int r = 0; r < numRows; r++)
        {
            for (int c = 0; c < numColumns; c++)
            {
                BatterySocket socket = GetSocket(r, c);
                int placeValue = (int)Mathf.Pow(10, numColumns - 1 - c);

                if (socket != null && socket.currentBattery != null)
                {
                    rowValues[r] += socket.currentBattery.digitValue * placeValue;
                }
                else
                {
                    emptyRows.Add(r);
                    emptyPlaceValues.Add(placeValue);
                }
            }
        }

        if (emptyRows.Count == 0)
        {
            int currentTotal = 0;
            for (int r = 0; r < rowValues.Length; r++)
                currentTotal += rowValues[r];

            return currentTotal == currentTargetPower;
        }

        int[] availableCounts = null;
        if (availableDigits != null)
        {
            availableCounts = new int[10];
            for (int d = 0; d <= 9; d++)
                availableCounts[d] = availableDigits.TryGetValue(d, out int count) ? count : 0;
        }

        List<int> candidate = new List<int>();
        bool solved = TryAssignDigitsForCurrentBoard(
            0,
            rowValues,
            emptyRows,
            emptyPlaceValues,
            availableCounts,
            candidate);

        if (solved)
            requiredDigits.AddRange(candidate);

        return solved;
    }

    private bool TryAssignDigitsForCurrentBoard(
        int index,
        int[] rowValues,
        List<int> emptyRows,
        List<int> emptyPlaceValues,
        int[] availableCounts,
        List<int> chosenDigits)
    {
        if (index >= emptyRows.Count)
        {
            int total = 0;
            for (int r = 0; r < rowValues.Length; r++)
                total += rowValues[r];

            return total == currentTargetPower;
        }

        int row = emptyRows[index];
        int placeValue = emptyPlaceValues[index];

        for (int digit = 0; digit <= 9; digit++)
        {
            if (availableCounts != null && availableCounts[digit] <= 0)
                continue;

            if (availableCounts != null)
                availableCounts[digit]--;

            rowValues[row] += digit * placeValue;
            chosenDigits.Add(digit);

            if (TryAssignDigitsForCurrentBoard(index + 1, rowValues, emptyRows, emptyPlaceValues, availableCounts, chosenDigits))
                return true;

            chosenDigits.RemoveAt(chosenDigits.Count - 1);
            rowValues[row] -= digit * placeValue;

            if (availableCounts != null)
                availableCounts[digit]++;
        }

        return false;
    }

    private int GetRefillDigitValue()
    {
        // Count currently available digits from tray + sockets
        Dictionary<int, int> availableCounts = new Dictionary<int, int>();

        if (batteryTray != null)
        {
            foreach (Transform child in batteryTray)
            {
                BatteryIdentity battery = child.GetComponent<BatteryIdentity>();
                if (battery == null) continue;

                int digit = battery.digitValue;
                if (!availableCounts.ContainsKey(digit)) availableCounts[digit] = 0;
                availableCounts[digit]++;
            }
        }

        if (sockets != null)
        {
            foreach (BatterySocket socket in sockets)
            {
                if (socket == null || socket.currentBattery == null) continue;

                int digit = socket.currentBattery.digitValue;
                if (!availableCounts.ContainsKey(digit)) availableCounts[digit] = 0;
                availableCounts[digit]++;
            }
        }

        // Prioritize the currently active column (ones first, then tens...)
        int activeColumn = GetActiveColumnIndex();
        if (activeColumn >= 0 && solutionDigits != null)
        {
            Dictionary<int, int> requiredForColumn = new Dictionary<int, int>();
            for (int r = 0; r < numRows; r++)
            {
                int requiredDigit = solutionDigits[r, activeColumn];
                if (!requiredForColumn.ContainsKey(requiredDigit)) requiredForColumn[requiredDigit] = 0;
                requiredForColumn[requiredDigit]++;
            }

            foreach (var pair in requiredForColumn)
            {
                int required = pair.Value;
                int available = availableCounts.ContainsKey(pair.Key) ? availableCounts[pair.Key] : 0;
                if (available < required)
                    return pair.Key;
            }
        }

        // If any required solution digit is missing, spawn that first
        foreach (var pair in requiredDigitCounts)
        {
            int required = pair.Value;
            int available = availableCounts.ContainsKey(pair.Key) ? availableCounts[pair.Key] : 0;
            if (available < required)
                return pair.Key;
        }

        // Otherwise spawn a distractor
        return Random.Range(0, 10);
    }

    private int GetActiveColumnIndex()
    {
        if (sockets == null || sockets.Length == 0) return -1;

        // Ones-first flow: prioritize rightmost incomplete column.
        for (int c = numColumns - 1; c >= 0; c--)
        {
            bool columnComplete = true;
            for (int r = 0; r < numRows; r++)
            {
                BatterySocket socket = GetSocket(r, c);
                if (socket == null || socket.currentBattery == null)
                {
                    columnComplete = false;
                    break;
                }
            }

            if (!columnComplete)
                return c;
        }

        return -1;
    }

    private void ValidateTraySettings()
    {
        // Power Station uses a fixed 2x3 tray with 6 visible batteries.
        if (trayColumns < 3) trayColumns = 3;
        if (trayRows < 2) trayRows = 2;
        if (maxVisibleBatteries < 6) maxVisibleBatteries = 6;
    }

    private int GetTrayCapacity()
    {
        int byGrid = Mathf.Max(1, trayColumns * trayRows);
        int byLimit = Mathf.Max(1, maxVisibleBatteries);
        return Mathf.Min(byGrid, byLimit);
    }

    /// <summary>
    /// Recalculates and displays the live sum for each column.
    /// Shows carry indicators when a column sum >= 10.
    /// Tints column pipes green (match), red (over), or neutral.
    /// </summary>
    private void UpdateColumnSums()
    {
        EnsureColumnSumTextsInitialized();

        // Calculate raw column sums (before carry)
        int[] rawColumnSums = new int[numColumns];
        bool[] columnFullFlags = new bool[numColumns];
        bool[] columnHasAnyBattery = new bool[numColumns];

        for (int c = 0; c < numColumns; c++)
        {
            int colSum = 0;
            bool isFull = true;

            for (int r = 0; r < numRows; r++)
            {
                BatterySocket socket = GetSocket(r, c);
                if (socket != null && socket.currentBattery != null)
                {
                    colSum += socket.currentBattery.digitValue;
                    columnHasAnyBattery[c] = true;
                }
                else
                    isFull = false;
            }

            rawColumnSums[c] = colSum;
            columnFullFlags[c] = isFull;
        }

        // Carry flow right -> left (ones to tens to hundreds)
        int[] carryInForColumn = new int[numColumns];
        int[] carryOutFromColumn = new int[numColumns];
        int carryIn = 0;
        for (int c = numColumns - 1; c >= 0; c--)
        {
            carryInForColumn[c] = carryIn;
            int total = rawColumnSums[c] + carryIn;

            if (columnFullFlags[c])
            {
                carryOutFromColumn[c] = total / 10; // carry OUT to the column on the left
                carryIn = carryOutFromColumn[c];
            }
            else
            {
                // Column not complete yet: do not cascade carry further left.
                carryOutFromColumn[c] = 0;
                carryIn = 0;
            }
        }

        // Reset all carry indicators first
        bool anyCarryVisible = false;

        for (int c = 0; c < carryTexts.Length; c++)
        {
            if (carryTexts[c] != null)
                carryTexts[c].gameObject.SetActive(false);
        }

        // Update column sum texts
        for (int c = 0; c < numColumns; c++)
        {
            int totalWithCarry = rawColumnSums[c] + carryInForColumn[c];
            bool isRightmostColumn = (c == numColumns - 1);

            if (c < columnSumTexts.Length && columnSumTexts[c] != null)
            {
                if (columnFullFlags[c])
                {
                    if (isRightmostColumn)
                    {
                        // Carry logic only for ones/rightmost column (e.g. 7+6=13 => show 3)
                        columnSumTexts[c].text = (totalWithCarry % 10).ToString();
                    }
                    else
                    {
                        // For left column(s), show full value (e.g. 8+5 => 13)
                        columnSumTexts[c].text = totalWithCarry.ToString();
                    }
                }
                else
                {
                    // Live running total for incomplete column, including carry-in.
                    bool hasValueToShow = columnHasAnyBattery[c] || carryInForColumn[c] > 0;
                    columnSumTexts[c].text = hasValueToShow ? totalWithCarry.ToString() : "0";
                }

                columnSumTexts[c].color = Color.white;
            }

            // Carry produced by this column is displayed above the column to its left
            if (isRightmostColumn && columnFullFlags[c] && carryOutFromColumn[c] > 0)
            {
                anyCarryVisible = true;

                int leftColumn = c - 1;
                if (leftColumn >= 0 && leftColumn < carryTexts.Length && carryTexts[leftColumn] != null)
                {
                    carryTexts[leftColumn].text = "+" + carryOutFromColumn[c].ToString();
                    carryTexts[leftColumn].gameObject.SetActive(true);
                }
            }

            // Tint the pipe image
            if (c < columnPipeImages.Length && columnPipeImages[c] != null)
            {
                bool columnFull = columnFullFlags[c];
                if (!columnFull)
                    columnPipeImages[c].color = new Color(0.5f, 0.8f, 1f, 0.5f); // Neutral light blue
                else
                {
                    columnPipeImages[c].color = Color.white;
                }
            }
        }

        if (numColumns >= 2)
            Debug.Log($"[PowerStation] Live sums -> C0:{rawColumnSums[0]} C1:{rawColumnSums[1]} | CarryInC0:{carryInForColumn[0]} CarryOutC1:{carryOutFromColumn[1]}");

        if (carryIndicatorObject != null)
            carryIndicatorObject.SetActive(anyCarryVisible);
    }

    // ═══════════════════════════════════════════
    //  SUBMIT — CHECK THE ANSWER
    // ═══════════════════════════════════════════

    public void OnSubmitPressed()
    {
        if (isProcessingResult) return;

        // 1. Verify all sockets are filled
        foreach (var socket in sockets)
        {
            if (socket.currentBattery == null)
            {
                Debug.Log("[PowerStation] Not all sockets filled yet!");
                return;
            }
        }

        // 2. Calculate the total using vertical addition
        int totalSum = 0;

        for (int r = 0; r < numRows; r++)
        {
            int addend = 0;
            for (int c = 0; c < numColumns; c++)
            {
                BatterySocket socket = GetSocket(r, c);
                if (socket != null && socket.currentBattery != null)
                {
                    int digit = socket.currentBattery.digitValue;
                    // c=0 is leftmost (highest place value)
                    int placeValue = (int)Mathf.Pow(10, numColumns - 1 - c);
                    addend += digit * placeValue;
                }
            }
            totalSum += addend;
            Debug.Log($"[PowerStation] Row {r} addend = {addend}");
        }

        Debug.Log($"[PowerStation] Total = {totalSum}, Target = {currentTargetPower}");

        // 3. Evaluate
        if (totalSum == currentTargetPower)
        {
            HandleCorrectSupply();
        }
        else if (totalSum > currentTargetPower)
        {
            HandleOverload();
        }
        else
        {
            HandleUnderload();
        }
    }

    private BatterySocket GetSocket(int row, int col)
    {
        if (sockets == null) return null;

        for (int i = 0; i < sockets.Length; i++)
        {
            BatterySocket socket = sockets[i];
            if (socket == null) continue;
            if (socket.row == row && socket.column == col)
                return socket;
        }

        return null;
    }

    // ═══════════════════════════════════════════
    //  OUTCOME HANDLERS
    // ═══════════════════════════════════════════

    private void HandleCorrectSupply()
    {
        HandleCorrectAnswer(); // BaseLevelController: score++, events

        if (audioSource != null && correctSound != null)
            audioSource.PlayOneShot(correctSound);

        SetBoardVisualsVisible(false);

        // Machine idle (correct)
        SetMachineIdleVisual();

        Debug.Log($"<color=green>[PowerStation] CORRECT! Turn {currentTurn}/{totalTurns} complete.</color>");

        // Check if all turns are done
        if (currentTurn >= totalTurns)
        {
            StartCoroutine(ShowVictoryThenCompleteLevel());
        }
        else
        {
            // Show brief success feedback, then move to next turn
            StartCoroutine(ShowCorrectThenNextTurn());
        }
    }

    private IEnumerator ShowCorrectThenNextTurn()
    {
        if (correctPanel != null)
        {
            isProcessingResult = true;
            Vector2 hiddenPos  = new Vector2(targetX, hiddenY);
            Vector2 visiblePos = new Vector2(targetX, visibleY);

            correctPanel.anchoredPosition = hiddenPos;
            correctPanel.gameObject.SetActive(true);

            yield return StartCoroutine(MovePanel(correctPanel, hiddenPos, visiblePos));
            yield return new WaitForSecondsRealtime(waitTime);
            yield return StartCoroutine(MovePanel(correctPanel, visiblePos, hiddenPos));

            correctPanel.gameObject.SetActive(false);
            isProcessingResult = false;
        }
        else
        {
            yield return new WaitForSecondsRealtime(1.0f);
        }

        StartNextTurn();
    }

    private void HandleOverload()
    {
        HandleWrongAnswer(); // BaseLevelController: mistake++

        if (audioSource != null && overloadSound != null)
            audioSource.PlayOneShot(overloadSound);

        SetBoardVisualsVisible(false);

        // Machine overload
        SetMachineOverloadVisual();

        StartCoroutine(ShowFeedbackThenReset(overloadPanel));

        Debug.Log("<color=red>[PowerStation] OVERLOAD! Machine explodes!</color>");
    }

    private void HandleUnderload()
    {
        HandleWrongAnswer();

        if (audioSource != null && underloadSound != null)
            audioSource.PlayOneShot(underloadSound);

        SetBoardVisualsVisible(false);

        // Machine underload
        SetMachineUnderloadVisual();

        StartCoroutine(ShowFeedbackThenReset(underloadPanel));

        Debug.Log("<color=yellow>[PowerStation] UNDERLOAD! Machine shuts down.</color>");
    }

    // ═══════════════════════════════════════════
    //  FEEDBACK PANEL ANIMATION (same pattern as Hungry Golem)
    // ═══════════════════════════════════════════

    private IEnumerator ShowFeedbackThenReset(RectTransform panel)
    {
        if (panel == null) yield break;
        isProcessingResult = true;

        Vector2 hiddenPos  = new Vector2(targetX, hiddenY);
        Vector2 visiblePos = new Vector2(targetX, visibleY);

        panel.anchoredPosition = hiddenPos;
        panel.gameObject.SetActive(true);

        // Slide in
        yield return StartCoroutine(MovePanel(panel, hiddenPos, visiblePos));
        yield return new WaitForSecondsRealtime(waitTime);
        // Slide out
        yield return StartCoroutine(MovePanel(panel, visiblePos, hiddenPos));

        panel.gameObject.SetActive(false);

        // Reset this turn's board (same target, let them try again)
        ResetCurrentTurn();
        isProcessingResult = false;
    }

    private IEnumerator MovePanel(RectTransform rect, Vector2 from, Vector2 to)
    {
        float duration = 0.5f;
        float elapsed  = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t     = elapsed / duration;
            float curve = t * t * (3f - 2f * t); // Smooth-step
            rect.anchoredPosition = Vector2.Lerp(from, to, curve);
            yield return null;
        }
        rect.anchoredPosition = to;
    }

    // ═══════════════════════════════════════════
    //  RESET (wrong answer — retry same target)
    // ═══════════════════════════════════════════

    private void ResetCurrentTurn()
    {
        SetBoardVisualsVisible(true);
        ClearAllSockets();

        // Re-generate batteries for the same target
        List<int> batteries = GenerateBatteryPool();
        SpawnBatteries(batteries);

        // Reset column sums and machine visual
        ResetColumnSumDisplay();

        ForceMachineIdleForTurnStart();
    }

    /// <summary>Resets all column sum texts, carry indicators, and pipe colors to default.</summary>
    private void ResetColumnSumDisplay()
    {
        for (int c = 0; c < columnSumTexts.Length; c++)
        {
            if (columnSumTexts[c] != null)
            {
                columnSumTexts[c].text = "0";
                columnSumTexts[c].color = Color.white;
            }
        }

        for (int c = 0; c < carryTexts.Length; c++)
        {
            if (carryTexts[c] != null)
                carryTexts[c].gameObject.SetActive(false);
        }

        if (carryIndicatorObject != null)
            carryIndicatorObject.SetActive(false);

        for (int c = 0; c < columnPipeImages.Length; c++)
        {
            if (columnPipeImages[c] != null)
                columnPipeImages[c].color = new Color(0.5f, 0.8f, 1f, 0.5f); // Neutral
        }
    }

    // ═══════════════════════════════════════════
    //  VICTORY
    // ═══════════════════════════════════════════

    private AudioSource ResolveVictoryAudioSource()
    {
        if (victoryAudioSource != null) return victoryAudioSource;
        if (audioSource != null) return audioSource;

        audioSource = GetComponent<AudioSource>();
        if (audioSource != null) return audioSource;

        if (machineImage != null)
        {
            AudioSource imageSource = machineImage.GetComponent<AudioSource>();
            if (imageSource != null)
            {
                audioSource = imageSource;
                return imageSource;
            }
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0f;
        return audioSource;
    }

    private void PlayVictoryAudio()
    {
        AudioClip clipToPlay = victoryChime;

        if (clipToPlay == null && AudioManager.Instance != null)
            clipToPlay = AudioManager.Instance.levelCompleteSound;

        if (clipToPlay == null)
        {
            Debug.LogWarning("[PowerStation] Victory audio skipped: no Victory Chime assigned and no AudioManager levelCompleteSound available.");
            return;
        }

        AudioSource source = ResolveVictoryAudioSource();
        if (source != null)
        {
            source.PlayOneShot(clipToPlay);
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayLevelComplete();
            return;
        }

        Debug.LogWarning("[PowerStation] Victory audio skipped: no AudioSource and no AudioManager available.");
    }

    private void ShowVictory()
    {
        SetBoardVisualsVisible(false);

        // Start animation + sound together
        SetMachineVictoryVisual();
        PlayVictoryAudio();

        if (confetti != null)  confetti.Play();
        if (sparks != null)    sparks.Play();

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    private IEnumerator ShowVictoryThenCompleteLevel()
    {
        isProcessingResult = true;

        ShowVictory();

        float waitDuration = Mathf.Max(0f, victoryAnimationDuration);
        float clipLength = GetVictoryClipLength();
        if (clipLength > 0f)
            waitDuration = Mathf.Max(waitDuration, clipLength);

        if (waitDuration > 0f)
            yield return new WaitForSecondsRealtime(waitDuration);

        EndLevel(true);
        isProcessingResult = false;
    }

    private float GetVictoryClipLength()
    {
        if (machineAnimator == null || machineAnimator.runtimeAnimatorController == null)
            return 0f;

        AnimationClip[] clips = machineAnimator.runtimeAnimatorController.animationClips;
        if (clips == null || clips.Length == 0)
            return 0f;

        for (int i = 0; i < clips.Length; i++)
        {
            AnimationClip clip = clips[i];
            if (clip == null) continue;

            if (string.Equals(clip.name, victoryStateName, System.StringComparison.OrdinalIgnoreCase) ||
                string.Equals(clip.name, "machine_victory", System.StringComparison.OrdinalIgnoreCase))
                return clip.length;
        }

        return 0f;
    }

    // ═══════════════════════════════════════════
    //  UTILITY
    // ═══════════════════════════════════════════

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
