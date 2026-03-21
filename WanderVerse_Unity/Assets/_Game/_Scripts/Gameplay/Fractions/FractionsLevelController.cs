using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class FractionsLevelController : BaseLevelController
{
    protected override GameType SupportedGameType => GameType.Fractions;

    public enum FractionMode { Halves, Quarters }

    [Header("--- Mode ---")]
    [Tooltip("Halves = Level 1 | Quarters = Level 2")]
    public FractionMode levelMode = FractionMode.Halves;

    [Header("--- Food Items (Drag items in order) ---")]
    public GameObject[] foodItems;
    private int currentItemIndex = 0;

    [Header("--- Guided Settings ---")]
    [Tooltip("First X items show dotted line(s). Rest do not.")]
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
    public float bubbleDisplayTime = 3f;

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

    [Header("--- Audio ---")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip levelCompleteSound;

    [Header("--- Victory ---")]
    public GameObject victoryPanel;
    public ParticleSystem confetti;

    // -------------------------------------------------------
    // MESSAGES — HALVES (Level 1)
    // -------------------------------------------------------
    private string halvesWelcome  = "fldfyduo hd¨‍jd'''uu ;uhs l%ïn,a'uf. fílßhg Thdj idorfhka ms<s.kakjd'";
    private string halvesCorrect  = "kshuhs æ yßhgu ueÈka lemqjd æ'''";
    private string halvesWrong    = "yßhgu ueÈka lmkak W;aiy lrkak æ";
    private string halvesLastFive = "ශාබාශ්! දැන් රේඛාව නැතිව උත්සාහ කරන්න!";
    private string halvesComplete = "Thdkï ienEu olaYfhla ;uhs æ";

    private string[] halvesGuided = new string[]
    {
        "කේක් එක හරියටම බෙදා කපන්න!",
        "l=lS tl ueÈka lmkakæ",
        "mSid tl iudk follg lmkakæ",
        "mdka tl yßhgu ueÈka lmkakæ",
        "fpdl,Ü tl iudk follg lmkakæ"
    };

    private string[] halvesFree = new string[]
    {
        "fu;eka mgka ueo f¾Ldj fkdue;sj W;aiy lrkak æ",
        "මැද කොතැනද?",
        "මධ්‍යය සොයා ගන්න!",
        "සමාන කොටස් දෙකකට කපන්න!",
        "Tng l< yelshsæ"
    };

    // -------------------------------------------------------
    // MESSAGES — QUARTERS (Level 2)
    // -------------------------------------------------------
    private string quartersWelcome   = "දැන් කාර්තු හතරට කපමු!";
    private string quartersFirstCut  = "පළමුව සිරස් කොටසට කපන්න!";
    private string quartersSecondCut = "ශාබාශ්! දැන් තිරස් කොටසට කපන්න!";
    private string quartersServe     = "හොඳයි! දැන් කොටස් තහඩුවට දමන්න!";
    private string quartersWrong     = "හරියටම මැදින් කපන්න!";
    private string quartersComplete  = "අපූරුයි! සියලු ඇණවුම් ලබා දුන්නා!";

    private string[] quartersOrders = new string[]
    {
        "කාර්තු එකක් දෙන්න!",
        "කාර්තු දෙකක් දෙන්න!",
        "කාර්තු හතරම දෙන්න!",
        "කාර්තු එකක් දෙන්න!",
        "කාර්තු දෙකක් දෙන්න!",
        "කාර්තු හතරම දෙන්න!"
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
    // MR CRUMBLE ENTRANCE
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
    // QUARTER CUTTER CALLBACKS
    // ================================================================
    public void OnFirstCutDone()
    {
        StartCoroutine(ShowBubble(quartersSecondCut, bubbleDisplayTime));

        if (currentItemIndex < guidedItemCount)
        {
            GameObject item = foodItems[currentItemIndex];
            Transform hLine = item.transform.Find("DottedLineH");
            if (hLine != null) hLine.gameObject.SetActive(true);
        }
    }

    public void OnBothCutsDone()
    {
        if (audioSource != null && correctSound != null)
            audioSource.PlayOneShot(correctSound);

        StartCoroutine(HappyReaction());
        StartCoroutine(ShowBubble(quartersServe, 2f));

        if (plate != null) plate.SetActive(true);
    }

    public void OnPiecePlaced()
    {
        placedPieces++;

        if (placedPieces >= requiredPieces)
        {
            HandleCorrectAnswer();
            StartCoroutine(ShowBubble("Order complete! Well done!", 2f));
            StartCoroutine(HappyReaction());

            if (orderSign != null)   orderSign.SetActive(false);
            if (plate != null)       plate.SetActive(false);
            if (greenSwitch != null) greenSwitch.SetActive(true);
        }
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

    public void ShowGreenSwitch()
    {
        if (greenSwitch != null) greenSwitch.SetActive(true);
    }

    // ================================================================
    // ALL ITEMS DONE
    // ================================================================
    private void AllItemsDone()
    {
        Debug.Log("[FractionsLevelController] LEVEL COMPLETE!");

        string msg = levelMode == FractionMode.Halves ? halvesComplete : quartersComplete;
        StartCoroutine(ShowBubble(msg, 3f));

        CheckWinCondition();

        if (audioSource != null && levelCompleteSound != null)
            audioSource.PlayOneShot(levelCompleteSound);
        if (confetti != null)
        {
            confetti.gameObject.SetActive(true);
            confetti.Play();
        }
        if (victoryPanel != null) victoryPanel.SetActive(true);

        StartCoroutine(CrumbleCelebration());
    }

    // ================================================================
    // MR CRUMBLE CELEBRATION — jumps happily at level end
    // ================================================================
    private IEnumerator CrumbleCelebration()
    {
        if (mrCrumbleRenderer == null) yield break;

        Vector3 origPos = mrCrumbleRenderer.transform.position;

        // Jump 4 times
        for (int i = 0; i < 4; i++)
        {
            mrCrumbleRenderer.sprite = crumbleHappy;

            Vector3 jumpPos = origPos + new Vector3(0, 0.6f, 0);
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 8f;
                mrCrumbleRenderer.transform.position = Vector3.Lerp(origPos, jumpPos, t);
                yield return null;
            }

            t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime * 8f;
                mrCrumbleRenderer.transform.position = Vector3.Lerp(jumpPos, origPos, t);
                yield return null;
            }

            mrCrumbleRenderer.sprite = crumbleIdle;
            yield return new WaitForSeconds(0.1f);
        }

        // Stay happy at the end
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

        StartCoroutine(ShowBubble(
            levelMode == FractionMode.Halves ? halvesCorrect : "Perfect!",
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
    // SPEECH BUBBLE
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