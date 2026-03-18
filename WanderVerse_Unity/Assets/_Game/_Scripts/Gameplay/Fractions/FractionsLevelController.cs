using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class FractionsLevelController : BaseLevelController
{
    protected override GameType SupportedGameType => GameType.Fractions;

    // -------------------------------------------------------
    // MODE — set this in Inspector per level
    // -------------------------------------------------------
    public enum FractionMode { Halves, Quarters }

    [Header("--- Mode ---")]
    [Tooltip("Halves = Level 1 | Quarters = Level 2")]
    public FractionMode levelMode = FractionMode.Halves;

    // -------------------------------------------------------
    // FOOD ITEMS
    // -------------------------------------------------------
    [Header("--- Food Items (Drag items in order) ---")]
    public GameObject[] foodItems;
    private int currentItemIndex = 0;

    // -------------------------------------------------------
    // GUIDED SETTINGS
    // -------------------------------------------------------
    [Header("--- Guided Settings ---")]
    [Tooltip("First X items show dotted line(s). Rest do not.")]
    public int guidedItemCount = 5;

    // -------------------------------------------------------
    // REFERENCES
    // -------------------------------------------------------
    [Header("--- References ---")]
    public KnifeCutter knifeCutter;         // Used in Halves mode
    public QuarterCutter quarterCutter;     // Used in Quarters mode
    public GameObject greenSwitch;

    // -------------------------------------------------------
    // MR CRUMBLE
    // -------------------------------------------------------
    [Header("--- Mr. Crumble ---")]
    public SpriteRenderer mrCrumbleRenderer;
    public Sprite crumbleIdle;
    public Sprite crumbleHappy;
    public Sprite crumbleAngry;

    [Header("--- Mr. Crumble Entrance ---")]
    public Vector3 crumbleFinalPosition = new Vector3(-6f, -2.41f, 0f);
    public float entranceSpeed = 2f;

    // -------------------------------------------------------
    // SPEECH BUBBLE
    // -------------------------------------------------------
    [Header("--- Speech Bubble ---")]
    public GameObject speechBubble;
    public TextMeshProUGUI bubbleText;
    public float bubbleDisplayTime = 3f;

    // -------------------------------------------------------
    // QUARTERS ONLY — Order Sign & Plate
    // -------------------------------------------------------
    [Header("--- Quarters Mode Only ---")]
    public GameObject orderSign;
    public TextMeshProUGUI orderSignText;
    public Image orderSignImage;
    public Sprite quarterSprite;
    public Sprite halfSprite;
    public Sprite wholeSprite;
    public GameObject plate;

    private int requiredPieces = 0;
    private int placedPieces   = 0;

    private int[] orderAmounts = new int[] { 1, 2, 4, 1, 2, 4 };

    // -------------------------------------------------------
    // AUDIO
    // -------------------------------------------------------
    [Header("--- Audio ---")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip levelCompleteSound;

    // -------------------------------------------------------
    // VICTORY
    // -------------------------------------------------------
    [Header("--- Victory ---")]
    public GameObject victoryPanel;
    public ParticleSystem confetti;

    // -------------------------------------------------------
    // MESSAGES — HALVES (Level 1) — unchanged
    // -------------------------------------------------------
    private string halvesWelcome    = "Welcome to Crumble's Bakery! Let's cut food in half!";
    private string halvesCorrect    = "Perfect! Right down the middle!";
    private string halvesWrong      = "Oops! Try to cut through the center!";
    private string halvesLastFive   = "Great job! Now try without the guide line!";
    private string halvesComplete   = "Amazing! You cut everything in half!";

    private string[] halvesGuided = new string[]
    {
        "Cut the cake exactly in half!",
        "Slice right through the middle!",
        "Follow the dotted line!",
        "Cut it into two equal parts!",
        "Right down the center!"
    };

    private string[] halvesFree = new string[]
    {
        "Now try without the guide!",
        "Find the middle yourself!",
        "Where is the center?",
        "Cut it equally in two!",
        "You can do it!"
    };

    // -------------------------------------------------------
    // MESSAGES — QUARTERS (Level 2)
    // -------------------------------------------------------
    private string quartersWelcome   = "Now let's cut into QUARTERS - 4 equal pieces!";
    private string quartersFirstCut  = "First cut - slice it in half horizontally!";
    private string quartersSecondCut = "Great! Now cut it vertically for 4 quarters!";
    private string quartersServe     = "Perfect quarters! Now drag the pieces to the plate!";
    private string quartersWrong     = "Try to cut through the exact middle!";
    private string quartersComplete  = "Amazing! All orders served perfectly!";

    private string[] quartersOrders = new string[]
    {
        "Give me 1 quarter please!",
        "Give me 2 quarters please!",
        "Give me all 4 quarters please!",
        "Give me 1 quarter please!",
        "Give me 2 quarters please!",
        "Give me all 4 quarters please!"
    };

    // ================================================================
    // INIT
    // ================================================================
    protected override void InitializeLevel()
    {
        foreach (GameObject item in foodItems)
            item.SetActive(false);

        if (greenSwitch != null)  greenSwitch.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (confetti != null)     confetti.Stop();
        if (speechBubble != null) speechBubble.SetActive(false);
        if (orderSign != null)    orderSign.SetActive(false);
        if (plate != null)        plate.SetActive(false);

        if (mrCrumbleRenderer != null)
            mrCrumbleRenderer.sprite = crumbleIdle;

        StartCoroutine(MrCrumbleEntrance());

        Debug.Log($"[FractionsLevelController] Started in {levelMode} mode.");
    }

    // ================================================================
    // MR CRUMBLE ENTRANCE — unchanged from Level 1
    // ================================================================
    private IEnumerator MrCrumbleEntrance()
    {
        if (mrCrumbleRenderer == null) { ShowCurrentItem(); yield break; }

        Vector3 offScreen = new Vector3(-15f, crumbleFinalPosition.y, 0f);
        mrCrumbleRenderer.transform.position = offScreen;

        yield return new WaitForSeconds(0.3f);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * entranceSpeed;
            float s = t * t * (3f - 2f * t);
            mrCrumbleRenderer.transform.position = Vector3.Lerp(offScreen, crumbleFinalPosition, s);
            yield return null;
        }
        mrCrumbleRenderer.transform.position = crumbleFinalPosition;

        Vector3 bounceUp = crumbleFinalPosition + new Vector3(0, 0.3f, 0);
        t = 0;
        while (t < 1f) { t += Time.deltaTime * 6f; mrCrumbleRenderer.transform.position = Vector3.Lerp(crumbleFinalPosition, bounceUp, t); yield return null; }
        t = 0;
        while (t < 1f) { t += Time.deltaTime * 6f; mrCrumbleRenderer.transform.position = Vector3.Lerp(bounceUp, crumbleFinalPosition, t); yield return null; }

        yield return new WaitForSeconds(0.2f);

        string welcome = levelMode == FractionMode.Halves ? halvesWelcome : quartersWelcome;
        yield return StartCoroutine(ShowBubble(welcome, bubbleDisplayTime));

        ShowCurrentItem();
    }

    // ================================================================
    // SHOW CURRENT ITEM
    // ================================================================
    private void ShowCurrentItem()
    {
        if (currentItemIndex >= foodItems.Length) { AllItemsDone(); return; }

        GameObject item   = foodItems[currentItemIndex];
        bool isGuided     = currentItemIndex < guidedItemCount;
        item.SetActive(true);

        if (levelMode == FractionMode.Halves)
            ShowItemHalves(item, isGuided);
        else
            ShowItemQuarters(item, isGuided);

        Debug.Log($"[FractionsLevelController] Item {currentItemIndex + 1}/{foodItems.Length} | {levelMode} | Guided: {isGuided}");
    }

    // ----------------------------------------------------------------
    // HALVES — unchanged from Level 1
    // ----------------------------------------------------------------
    private void ShowItemHalves(GameObject item, bool isGuided)
    {
        if (currentItemIndex == guidedItemCount)
            StartCoroutine(ShowBubble(halvesLastFive, bubbleDisplayTime));

        Transform dottedLine = item.transform.Find("DottedLine");
        if (dottedLine != null) dottedLine.gameObject.SetActive(isGuided);

        if (knifeCutter != null) knifeCutter.SetCurrentItem(item);

        if (currentItemIndex != guidedItemCount)
        {
            string msg = isGuided
                ? halvesGuided[Mathf.Min(currentItemIndex, halvesGuided.Length - 1)]
                : halvesFree[Mathf.Min(currentItemIndex - guidedItemCount, halvesFree.Length - 1)];

            StartCoroutine(ShowBubble(msg, bubbleDisplayTime));
        }
    }

    // ----------------------------------------------------------------
    // QUARTERS — new for Level 2
    // ----------------------------------------------------------------
    private void ShowItemQuarters(GameObject item, bool isGuided)
    {
        Transform hLine = item.transform.Find("DottedLineH");
        Transform vLine = item.transform.Find("DottedLineV");
        if (vLine != null) vLine.gameObject.SetActive(isGuided); // vertical shows first
        if (hLine != null) hLine.gameObject.SetActive(false);    // horizontal hidden until second cut

        if (quarterCutter != null) quarterCutter.SetCurrentItem(item, isGuided);

        requiredPieces = orderAmounts[Mathf.Min(currentItemIndex, orderAmounts.Length - 1)];
        placedPieces   = 0;

        StartCoroutine(ShowQuarterOrderSequence());
    }

    private IEnumerator ShowQuarterOrderSequence()
    {
        yield return StartCoroutine(ShowBubble(quartersFirstCut, bubbleDisplayTime));

        ShowOrderSign(requiredPieces);

        string orderMsg = quartersOrders[Mathf.Min(currentItemIndex, quartersOrders.Length - 1)];
        yield return StartCoroutine(ShowBubble(orderMsg, bubbleDisplayTime));
    }

    private void ShowOrderSign(int pieces)
    {
        if (orderSign == null) return;
        orderSign.SetActive(true);

        if (orderSignText != null)
        {
            switch (pieces)
            {
                case 1: orderSignText.text = "1/4"; break;
                case 2: orderSignText.text = "1/2"; break;
                case 4: orderSignText.text = "1";   break;
            }
        }

        if (orderSignImage != null)
        {
            switch (pieces)
            {
                case 1: orderSignImage.sprite = quarterSprite; break;
                case 2: orderSignImage.sprite = halfSprite;    break;
                case 4: orderSignImage.sprite = wholeSprite;   break;
            }
        }
    }

    // ================================================================
    // CALLED BY QuarterCutter — first cut done
    // ================================================================
    public void OnFirstCutDone()
    {
        StartCoroutine(ShowBubble(quartersSecondCut, bubbleDisplayTime));

        if (currentItemIndex < guidedItemCount)
        {
            GameObject item = foodItems[currentItemIndex];
            Transform hLine = item.transform.Find("DottedLineH");
            if (hLine != null) hLine.gameObject.SetActive(true); // horizontal shows after first cut
        }
    }

    // ================================================================
    // CALLED BY QuarterCutter — both cuts done
    // ================================================================
    public void OnBothCutsDone()
    {
        if (audioSource != null && correctSound != null)
            audioSource.PlayOneShot(correctSound);

        StartCoroutine(HappyReaction());
        StartCoroutine(ShowBubble(quartersServe, 2f));

        if (plate != null) plate.SetActive(true);
    }

    // ================================================================
    // CALLED BY PieceDragger — piece placed on plate
    // ================================================================
    public void OnPiecePlaced()
    {
        placedPieces++;
        Debug.Log($"[FractionsLevelController] Pieces placed: {placedPieces}/{requiredPieces}");

        if (placedPieces >= requiredPieces)
        {
            HandleCorrectAnswer();
            StartCoroutine(ShowBubble("Order complete! Well done!", 2f));
            StartCoroutine(HappyReaction());

            if (orderSign != null) orderSign.SetActive(false);
            if (plate != null)     plate.SetActive(false);
            if (greenSwitch != null) greenSwitch.SetActive(true);
        }
    }

    // ================================================================
    // CORRECT CUT — Halves mode
    // ================================================================
    public void OnCorrectCut()
    {
        HandleCorrectAnswer();
        if (audioSource != null && correctSound != null)
            audioSource.PlayOneShot(correctSound);

        StartCoroutine(HappyReaction());
    }

    // ================================================================
    // WRONG CUT — both modes
    // ================================================================
    public void OnWrongCut()
    {
        HandleWrongAnswer();
        if (audioSource != null && wrongSound != null)
            audioSource.PlayOneShot(wrongSound);

        StartCoroutine(AngryReaction());
    }

    // ================================================================
    // GREEN SWITCH
    // ================================================================
    public void OnGreenSwitchPressed()
    {
        if (greenSwitch != null) greenSwitch.SetActive(false);
        if (currentItemIndex < foodItems.Length) foodItems[currentItemIndex].SetActive(false);

        currentItemIndex++;

        if (mrCrumbleRenderer != null) mrCrumbleRenderer.sprite = crumbleIdle;

        if (levelMode == FractionMode.Halves && knifeCutter != null)
            knifeCutter.ResetKnife();
        else if (levelMode == FractionMode.Quarters && quarterCutter != null)
            quarterCutter.ResetKnife();

        ShowCurrentItem();
    }

    // ================================================================
    // SHOW GREEN SWITCH — called by KnifeCutter after halves reaction
    // ================================================================
    public void ShowGreenSwitch()
    {
        if (greenSwitch != null) greenSwitch.SetActive(true);
    }

    // ================================================================
    // ALL DONE
    // ================================================================
    private void AllItemsDone()
    {
        Debug.Log("[FractionsLevelController] LEVEL COMPLETE!");

        string msg = levelMode == FractionMode.Halves ? halvesComplete : quartersComplete;
        StartCoroutine(ShowBubble(msg, 3f));

        CheckWinCondition();

        if (audioSource != null && levelCompleteSound != null)
            audioSource.PlayOneShot(levelCompleteSound);
        if (confetti != null)     confetti.Play();
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }

    // ================================================================
    // MR CRUMBLE REACTIONS — unchanged from Level 1
    // ================================================================
    private IEnumerator HappyReaction()
    {
        if (mrCrumbleRenderer == null) yield break;
        mrCrumbleRenderer.sprite = crumbleHappy;

        StartCoroutine(ShowBubble(
            levelMode == FractionMode.Halves ? halvesCorrect : "Perfect quarters!",
            2f));

        Vector3 orig = mrCrumbleRenderer.transform.position;
        Vector3 jump = orig + new Vector3(0, 0.5f, 0);
        float t = 0;
        while (t < 1f) { t += Time.deltaTime * 5f; mrCrumbleRenderer.transform.position = Vector3.Lerp(orig, jump, t); yield return null; }
        t = 0;
        while (t < 1f) { t += Time.deltaTime * 5f; mrCrumbleRenderer.transform.position = Vector3.Lerp(jump, orig, t); yield return null; }

        yield return new WaitForSeconds(0.5f);

        if (levelMode == FractionMode.Halves)
            ShowGreenSwitch();

        mrCrumbleRenderer.sprite = crumbleIdle;
    }

    private IEnumerator AngryReaction()
    {
        if (mrCrumbleRenderer == null) yield break;
        mrCrumbleRenderer.sprite = crumbleAngry;

        StartCoroutine(ShowBubble(
            levelMode == FractionMode.Halves ? halvesWrong : quartersWrong,
            2f));

        Vector3 orig    = mrCrumbleRenderer.transform.position;
        float   elapsed = 0f;
        while (elapsed < 0.8f)
        {
            float x = orig.x + Mathf.Sin(elapsed * 40f) * 0.12f;
            mrCrumbleRenderer.transform.position = new Vector3(x, orig.y, orig.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mrCrumbleRenderer.transform.position = orig;
        yield return new WaitForSeconds(1f);
        mrCrumbleRenderer.sprite = crumbleIdle;
    }

    // ================================================================
    // SPEECH BUBBLE — unchanged from Level 1
    // ================================================================
    private IEnumerator ShowBubble(string message, float duration)
    {
        if (speechBubble == null || bubbleText == null) yield break;

        bubbleText.text = message;
        speechBubble.SetActive(true);

        yield return StartCoroutine(BubbleFade(0f, 1f, 0.3f));
        yield return new WaitForSeconds(duration);
        yield return StartCoroutine(BubbleFade(1f, 0f, 0.3f));

        speechBubble.SetActive(false);
    }

    private IEnumerator BubbleFade(float from, float to, float dur)
    {
        if (bubbleText == null) yield break;
        float t = 0;
        Color col = bubbleText.color;
        while (t < 1f)
        {
            t += Time.deltaTime / dur;
            col.a = Mathf.Lerp(from, to, t);
            bubbleText.color = col;
            yield return null;
        }
    }
}