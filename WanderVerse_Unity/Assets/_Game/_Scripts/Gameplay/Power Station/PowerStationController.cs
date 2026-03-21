using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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
    public string idleStateName = "Machine_Idle";
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

    [Header("--- Submit ---")]
    public Button submitButton;

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

    private void SetMachineIdleVisual()
    {
        if (machineAnimator != null)
        {
            machineAnimator.ResetTrigger(overloadTriggerName);
            machineAnimator.ResetTrigger(underloadTriggerName);
            machineAnimator.ResetTrigger(victoryTriggerName);
            machineAnimator.Play(idleStateName, 0, 0f);
            return;
        }

        if (machineImage != null && machineIdle != null)
            machineImage.sprite = machineIdle;
    }

    private void SetMachineOverloadVisual()
    {
        if (machineAnimator != null)
        {
            machineAnimator.SetTrigger(overloadTriggerName);
            return;
        }

        if (machineImage != null && machineOverload != null)
            machineImage.sprite = machineOverload;
    }

    private void SetMachineUnderloadVisual()
    {
        if (machineAnimator != null)
        {
            machineAnimator.SetTrigger(underloadTriggerName);
            return;
        }

        if (machineImage != null && machineUnderload != null)
            machineImage.sprite = machineUnderload;
    }

    private void SetMachineVictoryVisual()
    {
        if (machineAnimator != null)
        {
            machineAnimator.SetTrigger(victoryTriggerName);
            return;
        }

        if (machineImage != null && machineVictory != null)
            machineImage.sprite = machineVictory;
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
        // Read configurable values from the ScriptableObject
        totalTurns = levelData.totalTurns;
        numRows    = levelData.batteryRows;
        currentTurn = 0;

        EnsureSocketsInitialized();
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

    private void EnsureSocketsInitialized()
    {
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

            if (allValid) return;
        }

        List<BatterySocket> discovered = new List<BatterySocket>();

        // 1) Existing BatterySocket components in scene
        BatterySocket[] existing = FindObjectsByType<BatterySocket>(FindObjectsSortMode.None);
        if (existing != null && existing.Length > 0)
            discovered.AddRange(existing);

        // 2) Fallback: add BatterySocket to objects named Socket_Rx_Cy
        if (discovered.Count == 0)
        {
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
                discovered.Add(socket);
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

        // 1. Generate a random target power within the configured range
        currentTargetPower = Random.Range(levelData.minTargetPower, levelData.maxTargetPower + 1);

        // 2. Figure out how many columns (digits) the target has
        numColumns = currentTargetPower.ToString().Length;

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
        SetMachineIdleVisual();

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
        List<int> pool = new List<int>();

        // --- Step A: Build valid addends whose sum == currentTargetPower ---
        // We generate (numRows) random addends that sum to the target.
        int[] addends = GenerateAddends(currentTargetPower, numRows);

        // Extract individual digits from each addend and add to the pool
        for (int r = 0; r < addends.Length; r++)
        {
            string addendStr = addends[r].ToString().PadLeft(numColumns, '0');
            for (int c = 0; c < addendStr.Length; c++)
            {
                int digit = int.Parse(addendStr[c].ToString());
                pool.Add(digit);
            }
        }

        // --- Step B: Add distractor batteries ---
        int maxDistractorsByCapacity = Mathf.Max(0, maxVisibleBatteries - pool.Count);
        int distractorCount = Mathf.Min(levelData.extraDistractorBatteries, maxDistractorsByCapacity);
        for (int i = 0; i < distractorCount; i++)
        {
            pool.Add(Random.Range(0, 10));
        }

        // --- Step C: Shuffle the pool so correct digits aren't always first ---
        ShuffleList(pool);

        // Hard cap to tray capacity while preserving all required digits added above
        if (pool.Count > maxVisibleBatteries)
            pool = pool.GetRange(0, maxVisibleBatteries);

        return pool;
    }

    private void ConfigureBatteryTrayLayout()
    {
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

        // Destroy old batteries in the tray
        foreach (Transform child in batteryTray)
            Destroy(child.gameObject);

        // Instantiate one battery UI element per value
        int maxToSpawn = Mathf.Min(values.Count, Mathf.Max(1, trayColumns * trayRows), maxVisibleBatteries);
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
        Debug.Log("[PowerStation] Battery placed — column sums updated.");
    }

    /// <summary>Called after a battery successfully snaps into a socket.</summary>
    public void OnBatterySnappedToSocket()
    {
        RefillTrayAfterSnap();
        OnBatteryPlaced();
    }

    private void RefillTrayAfterSnap()
    {
        if (batteryTray == null || batteryPrefab == null) return;

        int trayCapacity = Mathf.Min(Mathf.Max(1, trayColumns * trayRows), maxVisibleBatteries);
        if (batteryTray.childCount >= trayCapacity) return;

        GameObject go = Instantiate(batteryPrefab, batteryTray);
        BatteryIdentity id = go.GetComponent<BatteryIdentity>();
        if (id != null)
            id.Setup(Random.Range(0, 10));

        if (useManualTrayGridLayout)
            ApplyManualTrayGridLayout();
    }

    /// <summary>
    /// Recalculates and displays the live sum for each column.
    /// Shows carry indicators when a column sum >= 10.
    /// Tints column pipes green (match), red (over), or neutral.
    /// </summary>
    private void UpdateColumnSums()
    {
        string targetStr = currentTargetPower.ToString().PadLeft(numColumns, '0');

        // Calculate raw column sums (before carry)
        int[] rawColumnSums = new int[numColumns];
        bool[] columnFullFlags = new bool[numColumns];

        for (int c = 0; c < numColumns; c++)
        {
            int colSum = 0;
            bool isFull = true;

            for (int r = 0; r < numRows; r++)
            {
                BatterySocket socket = GetSocket(r, c);
                if (socket != null && socket.currentBattery != null)
                    colSum += socket.currentBattery.digitValue;
                else
                    isFull = false;
            }

            rawColumnSums[c] = colSum;
            columnFullFlags[c] = isFull;
        }

        // Process carries from right to left (ones → tens → hundreds…)
        int[] carries = new int[numColumns];
        int[] finalDigits = new int[numColumns];

        for (int c = numColumns - 1; c >= 0; c--)
        {
            int total = rawColumnSums[c];

            // Add carry from the column to the right
            if (c < numColumns - 1)
                total += carries[c + 1] > 0 ? 0 : 0; // Carry comes FROM the right

            // Actually, carries flow left: column c receives carry from column c+1
            // But carry is generated BY column c and sent to column c-1
            // Let's recalculate properly:
            // Reset and do a clean right-to-left pass
        }

        // Clean right-to-left carry pass
        int carryIn = 0;
        for (int c = numColumns - 1; c >= 0; c--)
        {
            int total = rawColumnSums[c] + carryIn;
            finalDigits[c] = total % 10;
            carries[c] = total / 10;   // carry OUT to the column on the left
            carryIn = carries[c];
        }

        // Update column sum texts
        for (int c = 0; c < numColumns; c++)
        {
            bool columnFull = columnFullFlags[c];

            if (c < columnSumTexts.Length && columnSumTexts[c] != null)
            {
                if (!columnFull)
                {
                    // Show partial sum or "?" if column not full
                    int partial = rawColumnSums[c];
                    columnSumTexts[c].text = partial > 0 ? partial.ToString() : "?";
                    columnSumTexts[c].color = Color.white;
                }
                else
                {
                    // Show the final digit for this column
                    columnSumTexts[c].text = finalDigits[c].ToString();

                    // Color: green if matches target digit, red if not
                    int targetDigit = int.Parse(targetStr[c].ToString());
                    columnSumTexts[c].color = (finalDigits[c] == targetDigit) ? Color.green : Color.red;
                }
            }

            // Update carry indicators
            if (c < carryTexts.Length && carryTexts[c] != null)
            {
                if (carries[c] > 0 && columnFull)
                {
                    carryTexts[c].text = "+" + carries[c].ToString();
                    carryTexts[c].gameObject.SetActive(true);
                }
                else
                {
                    carryTexts[c].gameObject.SetActive(false);
                }
            }

            // Tint the pipe image
            if (c < columnPipeImages.Length && columnPipeImages[c] != null)
            {
                if (!columnFull)
                    columnPipeImages[c].color = new Color(0.5f, 0.8f, 1f, 0.5f); // Neutral light blue
                else
                {
                    int targetDigit = int.Parse(targetStr[c].ToString());
                    if (finalDigits[c] == targetDigit)
                        columnPipeImages[c].color = Color.green;
                    else
                        columnPipeImages[c].color = Color.red;
                }
            }
        }
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
        int index = row * numColumns + col;
        if (index >= 0 && index < sockets.Length)
            return sockets[index];
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

        // Machine idle (correct)
        SetMachineIdleVisual();

        Debug.Log($"<color=green>[PowerStation] CORRECT! Turn {currentTurn}/{totalTurns} complete.</color>");

        // Check if all turns are done
        if (currentTurn >= totalTurns)
        {
            // Level complete!
            CheckWinCondition();
            ShowVictory();
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
        ClearAllSockets();

        // Re-generate batteries for the same target
        List<int> batteries = GenerateBatteryPool();
        SpawnBatteries(batteries);

        // Reset column sums and machine visual
        ResetColumnSumDisplay();

        SetMachineIdleVisual();
    }

    /// <summary>Resets all column sum texts, carry indicators, and pipe colors to default.</summary>
    private void ResetColumnSumDisplay()
    {
        for (int c = 0; c < columnSumTexts.Length; c++)
        {
            if (columnSumTexts[c] != null)
            {
                columnSumTexts[c].text = "?";
                columnSumTexts[c].color = Color.white;
            }
        }

        for (int c = 0; c < carryTexts.Length; c++)
        {
            if (carryTexts[c] != null)
                carryTexts[c].gameObject.SetActive(false);
        }

        for (int c = 0; c < columnPipeImages.Length; c++)
        {
            if (columnPipeImages[c] != null)
                columnPipeImages[c].color = new Color(0.5f, 0.8f, 1f, 0.5f); // Neutral
        }
    }

    // ═══════════════════════════════════════════
    //  VICTORY
    // ═══════════════════════════════════════════

    private void ShowVictory()
    {
        SetMachineVictoryVisual();
        if (confetti != null)  confetti.Play();
        if (sparks != null)    sparks.Play();

        if (victoryAudioSource != null && victoryChime != null)
            victoryAudioSource.PlayOneShot(victoryChime);

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
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
