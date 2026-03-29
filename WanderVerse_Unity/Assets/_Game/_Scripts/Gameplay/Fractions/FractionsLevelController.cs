using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class FractionsLevelController : BaseLevelController
{
    protected override GameType SupportedGameType => GameType.Fractions;

    public enum FractionMode { Halves, Quarters }

    [Header("--- Mode ---")]
    public FractionMode levelMode = FractionMode.Halves;

    [Header("--- Food Items (Drag items in order) ---")]
    public GameObject[] foodItems;
    private int currentItemIndex = 0;

    [Header("--- Guided Settings ---")]
    public int guidedItemCount = 5;

    [Header("--- References ---")]
    public KnifeCutter knifeCutter;
    public QuarterCutter quarterCutter;
    public GameObject greenSwitch;

    [Header("--- Mr. Crumble ---")]
    public SpriteRenderer mrCrumbleRenderer;
    public Sprite crumbleIdle;
    public Sprite crumbleHappy;
    public Sprite crumbleAngry;

    [Header("--- Mr. Crumble Entrance ---")]
    public Vector3 crumbleFinalPosition = new Vector3(-6f, -2.41f, 0f);
    public float entranceSpeed = 2f;

    [Header("--- Speech Bubble ---")]
    public GameObject speechBubble;
    public TextMeshProUGUI bubbleText;
    public TextMeshProUGUI fractionText;
    public float bubbleDisplayTime = 3f;

    [Header("--- Quarters Mode Only ---")]
    public GameObject plate;

    private int[] orderAmounts = new int[] { 2, 1, 4, 1, 2, 4 };
    private int requiredPieces = 0;
    private int placedPieces   = 0;
    private int totalWrongs    = 0;
    private int chancesLeft    = 2;

    [Header("--- Audio ---")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip levelCompleteSound;

    [Header("--- Victory ---")]
    public GameObject victoryPanel;
    public ParticleSystem confetti;

    // HALVES messages
    private string halvesWelcome  = "fldfyduo hd¨‍jd'''uu ;uhs l%ïn,a'uf. fílßhg Thdj idorfhka ms<s.kakjd'";
    private string halvesCorrect  = "kshuhs æ yßhgu ueÈka lemqjd æ'''";
    private string halvesWrong    = "yßhgu ueÈka lmkak W;aiy lrkak æ";
    private string halvesLastFive = " f¾Ldj ke;sj W;aidy lrkakæ";
    private string halvesComplete = "Thdkï ienEu olaYfhla ;uhs æ";



    private string[] halvesGuided = new string[]
    {
        "flala tl yßhgu ueÈka lmkakæ",
        "fodvï f.äh folg lmkakæ",
        "iekaâúÉ tl folg lmkakæ",
        "lma flala tl folg lmkakæ",
        "mdka tl yßhgu ueÈka lmkakæ"
    };

    private string[] halvesFree = new string[]
    {
        "fu;eka mgka ueo f¾Ldj fkdue;sj W;aiy lrkak æ",
        "ueo fld;eko@",
        "uOHh fidhd .kakæ",
        "iudk fldgia follg lmkakæ",
        "Tng l< yelshsæ"
    };

    // QUARTERS messages
    private string quartersWelcome   = "uu ;uhs l%ïn,a'Thd,d ug Woõ lrkak ´k fï lEu wjYH .dkg ms.dkg od.kak'";
    private string quartersFirstCut  = "m<uqj isriaj lmkakæ";
    private string quartersSecondCut = " ;sriaj lmkakæ";
    private string quartersCorrect   = "fyd|hsæ ksjerÈ fldgia ÿkakdæ";
    private string quartersWrong     = "jerÈhsæ kej; W;aidy lrkakæ";
    private string quartersComplete  = "wmQrehsæ ish¨‍ weKjqï ,nd ÿkakdæ";

    [Header("--- Quarters Orders (6 questions) ---")]
    [TextArea(1, 3)]
    public string[] quartersOrders = new string[]
    {
        "flala tflka Nd.hla fokakæ ",
        "mSid tflka ld,la fokak ^1$4&'",
        "iïmQ¾K fvdakÜ tlu wjYHhs'",
        "fïflka ld,la ms.dfkka ;nkak'",
        "fpdl,Ü tflka Nd.hla wjYH fjkjd",
        "iïmQ¾K lmaflala tlu wjYHhs'"
    };

    // ================================================================
    // INIT
    // ================================================================
    protected override void InitializeLevel()
    {
        Screen.orientation = ScreenOrientation.LandscapeLeft;
        foreach (GameObject item in foodItems)
            item.SetActive(false);

        if (greenSwitch != null)  greenSwitch.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (confetti != null)     confetti.Stop();
        if (speechBubble != null) speechBubble.SetActive(false);
        if (plate != null)        plate.SetActive(false);

        if (mrCrumbleRenderer != null)
            mrCrumbleRenderer.sprite = crumbleIdle;

        totalWrongs = 0;
    }

    protected override void BeginLevel()
    {
        base.BeginLevel();
        
        StartCoroutine(MrCrumbleEntrance());
    }

    // ================================================================
    // MR CRUMBLE ENTRANCE
    // ================================================================
    private IEnumerator MrCrumbleEntrance()
    {
        if (mrCrumbleRenderer == null) { ShowCurrentItem(); yield break; }

        Vector3 offScreen = new Vector3(-15f, crumbleFinalPosition.y, 0f);
        mrCrumbleRenderer.transform.position = offScreen;

        yield return new WaitForSecondsRealtime(0.3f);

        float t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime * entranceSpeed;
            float s = t * t * (3f - 2f * t);
            mrCrumbleRenderer.transform.position = Vector3.Lerp(offScreen, crumbleFinalPosition, s);
            yield return null;
        }
        mrCrumbleRenderer.transform.position = crumbleFinalPosition;

        Vector3 bounceUp = crumbleFinalPosition + new Vector3(0, 0.3f, 0);
        t = 0;
        while (t < 1f) { t += Time.unscaledDeltaTime * 6f; mrCrumbleRenderer.transform.position = Vector3.Lerp(crumbleFinalPosition, bounceUp, t); yield return null; }
        t = 0;
        while (t < 1f) { t += Time.unscaledDeltaTime * 6f; mrCrumbleRenderer.transform.position = Vector3.Lerp(bounceUp, crumbleFinalPosition, t); yield return null; }

        yield return new WaitForSecondsRealtime(0.2f);

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

        GameObject item = foodItems[currentItemIndex];
        bool isGuided   = currentItemIndex < guidedItemCount;
        item.SetActive(true);

        if (levelMode == FractionMode.Halves)
            ShowItemHalves(item, isGuided);
        else
            ShowItemQuarters(item, isGuided);
    }

    private void ShowItemHalves(GameObject item, bool isGuided)
    {
        if (currentItemIndex == guidedItemCount)
            StartCoroutine(ShowBubble(halvesLastFive, bubbleDisplayTime));

        Transform dottedLine = item.transform.Find("DottedLine");
        if (dottedLine != null) dottedLine.gameObject.SetActive(isGuided);

        if (knifeCutter != null) knifeCutter.SetCurrentItem(item, isGuided);

        if (currentItemIndex != guidedItemCount)
        {
            string msg = isGuided
                ? halvesGuided[Mathf.Min(currentItemIndex, halvesGuided.Length - 1)]
                : halvesFree[Mathf.Min(currentItemIndex - guidedItemCount, halvesFree.Length - 1)];
            StartCoroutine(ShowBubble(msg, bubbleDisplayTime));
        }
    }

    private void ShowItemQuarters(GameObject item, bool isGuided)
    {
        Transform hLine = item.transform.Find("DottedLineH");
        Transform vLine = item.transform.Find("DottedLineV");
        if (vLine != null) vLine.gameObject.SetActive(isGuided);
        if (hLine != null) hLine.gameObject.SetActive(false);

        if (quarterCutter != null) quarterCutter.SetCurrentItem(item, isGuided);

        requiredPieces = orderAmounts[Mathf.Min(currentItemIndex, orderAmounts.Length - 1)];
        placedPieces   = 0;
        chancesLeft    = 2;

        StartCoroutine(ShowCuttingThenOrder());
    }

    private IEnumerator ShowCuttingThenOrder()
    {
        // Show first cut instruction
        yield return StartCoroutine(ShowBubble(quartersFirstCut, bubbleDisplayTime));
        // Now show order — stays permanently
        ShowOrderBubblePermanent();
    }

    // Shows bubble permanently — does NOT auto hide
    private void ShowOrderBubblePermanent()
    {
        if (speechBubble == null || bubbleText == null) return;

        string orderMsg = quartersOrders[Mathf.Min(currentItemIndex, quartersOrders.Length - 1)];
        Debug.Log($"[Order] Showing: {orderMsg}");

        bubbleText.text = orderMsg;

        // Set full alpha
        if (bubbleText != null)
        {
            Color c = bubbleText.color;
            c.a = 1f;
            bubbleText.color = c;
        }

        if (fractionText != null)
        {
            fractionText.text = requiredPieces == 2 ? "½" :
                                requiredPieces == 1 ? "¼" : "1";
            Color c = fractionText.color;
            c.a = 1f;
            fractionText.color = c;
            fractionText.gameObject.SetActive(true);
        }

        speechBubble.SetActive(true);
    }

    // ================================================================
    // QUARTER CUTTER CALLBACKS
    // ================================================================
    public void OnFirstCutDone()
    {
        StartCoroutine(ShowSecondCutThenOrder());

        if (currentItemIndex < guidedItemCount)
        {
            GameObject item = foodItems[currentItemIndex];
            Transform hLine = item.transform.Find("DottedLineH");
            if (hLine != null) hLine.gameObject.SetActive(true);
        }
    }

    private IEnumerator ShowSecondCutThenOrder()
    {
        yield return StartCoroutine(ShowBubble(quartersSecondCut, bubbleDisplayTime));
        ShowOrderBubblePermanent();
    }

    public void OnBothCutsDone()
    {
        if (audioSource != null && correctSound != null)
            audioSource.PlayOneShot(correctSound);

        if (plate != null) plate.SetActive(true);

        ShowOrderBubblePermanent();
        if (greenSwitch != null) greenSwitch.SetActive(true);
    }

    // ================================================================
    // PIECE PLACED
    // ================================================================
    public void OnPiecePlaced()
    {
        placedPieces++;
        Debug.Log($"[FractionsLevelController] Pieces placed: {placedPieces}/{requiredPieces}");
    }

    // ================================================================
    // GREEN SWITCH
    // ================================================================
    public void OnGreenSwitchPressed()
    {
        if (levelMode == FractionMode.Quarters)
        {
            if (placedPieces == requiredPieces)
            {
                if (greenSwitch != null) greenSwitch.SetActive(false);
                if (speechBubble != null) speechBubble.SetActive(false);
                HandleCorrectAnswer();
                if (audioSource != null && correctSound != null)
                    audioSource.PlayOneShot(correctSound);
                StartCoroutine(QuartersCorrectSequence());
            }
            else
            {
                HandleWrongAnswer();
                if (audioSource != null && wrongSound != null)
                    audioSource.PlayOneShot(wrongSound);
                if (greenSwitch != null) greenSwitch.SetActive(false);
                StartCoroutine(QuartersWrongSequence());
            }
            return;
        }

        // Halves
        if (greenSwitch != null) greenSwitch.SetActive(false);
        if (speechBubble != null) speechBubble.SetActive(false);
        if (currentItemIndex < foodItems.Length) foodItems[currentItemIndex].SetActive(false);
        currentItemIndex++;
        if (mrCrumbleRenderer != null) mrCrumbleRenderer.sprite = crumbleIdle;
        if (knifeCutter != null) knifeCutter.ResetKnife();
        ShowCurrentItem();
    }

    private IEnumerator QuartersCorrectSequence()
    {
        StartCoroutine(HappyReaction());
        yield return StartCoroutine(ShowBubble(quartersCorrect, bubbleDisplayTime));

        if (plate != null) plate.SetActive(false);
        if (currentItemIndex < foodItems.Length) foodItems[currentItemIndex].SetActive(false);
        currentItemIndex++;
        if (mrCrumbleRenderer != null) mrCrumbleRenderer.sprite = crumbleIdle;
        if (quarterCutter != null) quarterCutter.ResetKnife();
        ShowCurrentItem();
    }

    private IEnumerator QuartersWrongSequence()
    {
        totalWrongs++;
        chancesLeft--;

        StartCoroutine(AngryReaction());
        yield return StartCoroutine(ShowBubble(quartersWrong, bubbleDisplayTime));

        if (totalWrongs >= 6)
        {
            yield return StartCoroutine(ShowBubble("wfka hd¨‍jd'Thdf. wjia:d bjrhs'kej; W;aiy lrkak æ", 3f));
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            yield break;
        }

        if (chancesLeft <= 0)
        {
            if (plate != null) plate.SetActive(false);
            if (currentItemIndex < foodItems.Length) foodItems[currentItemIndex].SetActive(false);
            currentItemIndex++;
            if (mrCrumbleRenderer != null) mrCrumbleRenderer.sprite = crumbleIdle;
            if (quarterCutter != null) quarterCutter.ResetKnife();
            ShowCurrentItem();
            yield break;
        }

        placedPieces = 0;
        ResetQuarterPieces(foodItems[currentItemIndex]);
        ShowOrderBubblePermanent();
        if (greenSwitch != null) greenSwitch.SetActive(true);
    }

    private void ResetQuarterPieces(GameObject item)
    {
        string[] pieceNames = { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };
        foreach (string name in pieceNames)
        {
            Transform piece = item.transform.Find(name);
            if (piece != null)
            {
                PieceDragger dragger = piece.GetComponent<PieceDragger>();
                if (dragger != null) dragger.ResetPiece();
            }
        }
    }

    public void ShowGreenSwitch()
    {
        if (greenSwitch != null) greenSwitch.SetActive(true);
    }

    // ================================================================
    // KNIFE CUTTER CALLBACKS
    // ================================================================
    public void OnCorrectCut()
    {
        HandleCorrectAnswer();
        if (audioSource != null && correctSound != null)
            audioSource.PlayOneShot(correctSound);
        StartCoroutine(HappyReaction());
    }

    public void OnWrongCut()
    {
        HandleWrongAnswer();
        if (audioSource != null && wrongSound != null)
            audioSource.PlayOneShot(wrongSound);
        StartCoroutine(AngryReaction());
    }

    // ================================================================
    // ALL DONE
    // ================================================================
    private void AllItemsDone()
    {
        Debug.Log("[FractionsLevelController] LEVEL COMPLETE!");

        string msg = levelMode == FractionMode.Halves ? halvesComplete : quartersComplete;
        StartCoroutine(ShowBubble(msg, 3f));

        //CheckWinCondition();

        //if (audioSource != null && levelCompleteSound != null)
        //    audioSource.PlayOneShot(levelCompleteSound);

        //if (confetti != null)
        //{
        //    confetti.gameObject.SetActive(true);
        //    confetti.Play();
        //}

        //if (victoryPanel != null) victoryPanel.SetActive(true);

        //StartCoroutine(CrumbleCelebration());

        StartCoroutine(LevelCompleteSequence());
    }

    private IEnumerator LevelCompleteSequence()
    {
        StartCoroutine(CrumbleCelebration());

        string msg = levelMode == FractionMode.Halves ? halvesComplete : quartersComplete;

        yield return StartCoroutine(ShowBubble(msg, 3f));

        base.EndLevel(true);
    }

    // ================================================================
    // MR CRUMBLE CELEBRATION
    // ================================================================
    private IEnumerator CrumbleCelebration()
    {
        if (mrCrumbleRenderer == null) yield break;

        Vector3 origPos = mrCrumbleRenderer.transform.position;

        for (int i = 0; i < 4; i++)
        {
            mrCrumbleRenderer.sprite = crumbleHappy;
            Vector3 jumpPos = origPos + new Vector3(0, 0.6f, 0);
            float t = 0;
            while (t < 1f) { t += Time.unscaledDeltaTime * 8f; mrCrumbleRenderer.transform.position = Vector3.Lerp(origPos, jumpPos, t); yield return null; }
            t = 0;
            while (t < 1f) { t += Time.unscaledDeltaTime * 8f; mrCrumbleRenderer.transform.position = Vector3.Lerp(jumpPos, origPos, t); yield return null; }
            mrCrumbleRenderer.sprite = crumbleIdle;
            yield return new WaitForSecondsRealtime(0.1f);
        }

        mrCrumbleRenderer.sprite = crumbleHappy;
        mrCrumbleRenderer.transform.position = origPos;
    }

    // ================================================================
    // MR CRUMBLE REACTIONS
    // ================================================================
    private IEnumerator HappyReaction()
    {
        if (mrCrumbleRenderer == null) yield break;
        mrCrumbleRenderer.sprite = crumbleHappy;

        Vector3 orig = mrCrumbleRenderer.transform.position;
        Vector3 jump = orig + new Vector3(0, 0.5f, 0);
        float t = 0;
        while (t < 1f) { t += Time.unscaledDeltaTime * 5f; mrCrumbleRenderer.transform.position = Vector3.Lerp(orig, jump, t); yield return null; }
        t = 0;
        while (t < 1f) { t += Time.unscaledDeltaTime * 5f; mrCrumbleRenderer.transform.position = Vector3.Lerp(jump, orig, t); yield return null; }

        yield return new WaitForSecondsRealtime(0.5f);

        if (levelMode == FractionMode.Halves)
            ShowGreenSwitch();

        mrCrumbleRenderer.sprite = crumbleIdle;
    }

    private IEnumerator AngryReaction()
    {
        if (mrCrumbleRenderer == null) yield break;
        mrCrumbleRenderer.sprite = crumbleAngry;

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
        yield return new WaitForSecondsRealtime(1f);
        mrCrumbleRenderer.sprite = crumbleIdle;
    }

    // ================================================================
    // SPEECH BUBBLE — auto hides after duration
    // ================================================================
    private IEnumerator ShowBubble(string message, float duration)
    {
        if (speechBubble == null || bubbleText == null) yield break;

        bubbleText.text = message;
        speechBubble.SetActive(true);

        yield return StartCoroutine(BubbleFade(0f, 1f, 0.3f));
        yield return new WaitForSecondsRealtime(duration);
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
            t += Time.unscaledDeltaTime / dur;
            col.a = Mathf.Lerp(from, to, t);
            bubbleText.color = col;
            yield return null;
        }
    }
}