using UnityEngine;
using System.Collections;

public class FractionsLevelController : BaseLevelController
{
    protected override GameType SupportedGameType => GameType.Fractions;

    [Header("--- Food Items (Drag 10 in order) ---")]
    public GameObject[] foodItems;

    [Header("--- Guided Settings ---")]
    [Tooltip("First X items will show dotted line. Rest will not.")]
    public int guidedItemCount = 5;

    private int currentItemIndex = 0;

    [Header("--- References ---")]
    public KnifeCutter knifeCutter;
    public GameObject greenSwitch;

    [Header("--- Mr. Crumble ---")]
    public SpriteRenderer mrCrumbleRenderer;
    public Sprite crumbleIdle;
    public Sprite crumbleHappy;
    public Sprite crumbleAngry;

    [Header("--- Mr. Crumble Entrance ---")]
    [Tooltip("Where Mr Crumble stands normally - match his scene position")]
    public Vector3 crumbleFinalPosition = new Vector3(-6f, -1.5f, 0f);
    [Tooltip("How fast he slides in (higher = faster)")]
    public float entranceSpeed = 2f;

    [Header("--- Audio ---")]
    public AudioSource audioSource;
    public AudioClip correctSound;
    public AudioClip wrongSound;
    public AudioClip levelCompleteSound;

    [Header("--- Victory ---")]
    public GameObject victoryPanel;
    public ParticleSystem confetti;

    // -------------------------------------------------------
    // SETUP
    // -------------------------------------------------------
    protected override void InitializeLevel()
    {
        foreach (GameObject item in foodItems)
            item.SetActive(false);

        if (greenSwitch != null) greenSwitch.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (confetti != null) confetti.Stop();

        if (mrCrumbleRenderer != null)
            mrCrumbleRenderer.sprite = crumbleIdle;

        // Mr Crumble slides in first, then shows first item
        StartCoroutine(MrCrumbleEntrance());

        Debug.Log("[FractionsLevelController] Level started!");
    }

    // -------------------------------------------------------
    // MR CRUMBLE ENTRANCE ANIMATION
    // -------------------------------------------------------
    private IEnumerator MrCrumbleEntrance()
    {
        if (mrCrumbleRenderer == null)
        {
            ShowCurrentItem();
            yield break;
        }

        // Start off screen to the left
        Vector3 offScreen = new Vector3(-15f, crumbleFinalPosition.y, 0f);
        mrCrumbleRenderer.transform.position = offScreen;

        yield return new WaitForSeconds(0.3f);

        // Slide in smoothly with ease
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * entranceSpeed;
            float smoothT = t * t * (3f - 2f * t);
            mrCrumbleRenderer.transform.position = Vector3.Lerp(offScreen, crumbleFinalPosition, smoothT);
            yield return null;
        }

        mrCrumbleRenderer.transform.position = crumbleFinalPosition;

        // Bounce when he arrives
        Vector3 bounceUp = crumbleFinalPosition + new Vector3(0, 0.3f, 0);
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 6f;
            mrCrumbleRenderer.transform.position = Vector3.Lerp(crumbleFinalPosition, bounceUp, t);
            yield return null;
        }
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 6f;
            mrCrumbleRenderer.transform.position = Vector3.Lerp(bounceUp, crumbleFinalPosition, t);
            yield return null;
        }

        Debug.Log("[FractionsLevelController] Mr Crumble entrance done!");

        yield return new WaitForSeconds(0.3f);
        ShowCurrentItem();
    }

    // -------------------------------------------------------
    // SHOW ITEM
    // -------------------------------------------------------
    private void ShowCurrentItem()
    {
        if (currentItemIndex >= foodItems.Length)
        {
            AllItemsDone();
            return;
        }

        GameObject item = foodItems[currentItemIndex];
        item.SetActive(true);

        bool isGuided = currentItemIndex < guidedItemCount;

        Transform dottedLine = item.transform.Find("DottedLine");
        if (dottedLine != null)
            dottedLine.gameObject.SetActive(isGuided);

        if (knifeCutter != null)
            knifeCutter.SetCurrentItem(item);

        string mode = isGuided ? "GUIDED (dotted line ON)" : "FREE (no dotted line)";
        Debug.Log($"[FractionsLevelController] Item {currentItemIndex + 1}/{foodItems.Length}: {item.name} | {mode}");
    }

    // -------------------------------------------------------
    // GREEN SWITCH
    // -------------------------------------------------------
    public void OnGreenSwitchPressed()
    {
        if (greenSwitch != null) greenSwitch.SetActive(false);

        if (currentItemIndex < foodItems.Length)
            foodItems[currentItemIndex].SetActive(false);

        currentItemIndex++;

        if (mrCrumbleRenderer != null)
            mrCrumbleRenderer.sprite = crumbleIdle;

        if (knifeCutter != null)
            knifeCutter.ResetKnife();

        ShowCurrentItem();
    }

    // -------------------------------------------------------
    // CORRECT CUT
    // -------------------------------------------------------
    public void OnCorrectCut()
    {
        HandleCorrectAnswer();

        if (audioSource != null && correctSound != null)
            audioSource.PlayOneShot(correctSound);

        StartCoroutine(HappyReaction());
    }

    // -------------------------------------------------------
    // WRONG CUT
    // -------------------------------------------------------
    public void OnWrongCut()
    {
        HandleWrongAnswer();

        if (audioSource != null && wrongSound != null)
            audioSource.PlayOneShot(wrongSound);

        StartCoroutine(AngryReaction());
    }

    // -------------------------------------------------------
    // SHOW GREEN SWITCH
    // -------------------------------------------------------
    public void ShowGreenSwitch()
    {
        if (greenSwitch != null)
            greenSwitch.SetActive(true);
    }

    // -------------------------------------------------------
    // MR. CRUMBLE REACTIONS
    // -------------------------------------------------------
    private IEnumerator HappyReaction()
    {
        if (mrCrumbleRenderer != null)
            mrCrumbleRenderer.sprite = crumbleHappy;

        Vector3 originalPos = mrCrumbleRenderer.transform.position;
        Vector3 jumpPos = originalPos + new Vector3(0, 0.5f, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            mrCrumbleRenderer.transform.position = Vector3.Lerp(originalPos, jumpPos, t);
            yield return null;
        }
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            mrCrumbleRenderer.transform.position = Vector3.Lerp(jumpPos, originalPos, t);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        ShowGreenSwitch();
    }

    private IEnumerator AngryReaction()
    {
        if (mrCrumbleRenderer != null)
            mrCrumbleRenderer.sprite = crumbleAngry;

        Vector3 originalPos = mrCrumbleRenderer.transform.position;
        float elapsed = 0f;

        while (elapsed < 0.8f)
        {
            float x = originalPos.x + Mathf.Sin(elapsed * 40f) * 0.12f;
            mrCrumbleRenderer.transform.position = new Vector3(x, originalPos.y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        mrCrumbleRenderer.transform.position = originalPos;
        yield return new WaitForSeconds(1f);

        if (mrCrumbleRenderer != null)
            mrCrumbleRenderer.sprite = crumbleIdle;
    }

    // -------------------------------------------------------
    // ALL ITEMS DONE
    // -------------------------------------------------------
    private void AllItemsDone()
    {
        Debug.Log("[FractionsLevelController] All items done! LEVEL COMPLETE!");

        CheckWinCondition();

        if (audioSource != null && levelCompleteSound != null)
            audioSource.PlayOneShot(levelCompleteSound);

        if (confetti != null) confetti.Play();
        if (victoryPanel != null) victoryPanel.SetActive(true);
    }
}