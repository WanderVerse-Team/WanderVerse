using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class QuarterCutter : MonoBehaviour
{
    // -------------------------------------------------------
    // REFERENCES
    // -------------------------------------------------------
    [Header("--- Knife ---")]
    public Transform knifeTransform;
    private Vector3 knifeStartPosition;

    [Header("--- Cut Settings ---")]
    public float cutTolerance = 1.0f;
    public float knifeTipOffset = 0.5f;

    [Header("--- Level Controller ---")]
    public FractionsLevelController levelController;

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    private GameObject currentItem;
    private Vector3 itemCenter;
    private bool isGuided;

    private bool isDragging      = false;
    private bool knifeEnteredItem = false;

    // Cut tracking
    private bool firstCutDone  = false;
    private bool secondCutDone = false;
    private bool waitingForSecondCut = false;

    // Cut direction
    // Cut 1 = HORIZONTAL (knife moves left-right, check Y position)
    // Cut 2 = VERTICAL   (knife moves top-bottom, check X position)

    // -------------------------------------------------------
    // START
    // -------------------------------------------------------
    private void Start()
    {
        knifeStartPosition = knifeTransform.position;
    }

    // -------------------------------------------------------
    // SET CURRENT ITEM
    // -------------------------------------------------------
    public void SetCurrentItem(GameObject item, bool guided)
    {
        currentItem   = item;
        Collider2D col = item.GetComponent<Collider2D>();
        itemCenter = col != null ? col.bounds.center : item.transform.position;
        isGuided      = guided;
        firstCutDone  = false;
        secondCutDone = false;
        waitingForSecondCut = false;
        knifeEnteredItem = false;

        Debug.Log($"[QuarterCutter] Ready: {item.name} | Guided: {guided}");
    }

    // -------------------------------------------------------
    // RESET
    // -------------------------------------------------------
    public void ResetKnife()
    {
        isDragging           = false;
        firstCutDone         = false;
        secondCutDone        = false;
        waitingForSecondCut  = false;
        knifeEnteredItem     = false;
        knifeTransform.position = knifeStartPosition;
    }

    // -------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------
    private void Update()
    {
        if (secondCutDone) return;
        if (currentItem == null) return;

        var pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 screenPos = pointer.position.ReadValue();
        Vector2 worldPos  = Camera.main.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane));

        // Press
        if (pointer.press.wasPressedThisFrame && !isDragging)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null && hit.transform == knifeTransform)
            {
                isDragging       = true;
                knifeEnteredItem = false;
                Debug.Log("[QuarterCutter] Knife grabbed!");
            }
        }

        // Drag
        if (pointer.press.isPressed && isDragging)
        {
            knifeTransform.position = new Vector3(worldPos.x, worldPos.y, 0f);
            CheckIfInsideItem();
        }

        // Release
        if (pointer.press.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;

            if (knifeEnteredItem)
                ValidateCut();
            else
            {
                knifeTransform.position = knifeStartPosition;
                Debug.Log("[QuarterCutter] Knife released without touching item.");
            }
        }
    }

    // -------------------------------------------------------
    // CHECK IF KNIFE INSIDE ITEM
    // -------------------------------------------------------
    private void CheckIfInsideItem()
    {
        if (currentItem == null) return;

        Collider2D col = currentItem.GetComponent<Collider2D>();
        if (col == null) return;

        Vector2 tip = GetKnifeTip();
        if (col.bounds.Contains(new Vector3(tip.x, tip.y, 0f)))
            knifeEnteredItem = true;
    }

    private Vector2 GetKnifeTip()
    {
        Vector3 tip = knifeTransform.position + knifeTransform.up * knifeTipOffset;
        return new Vector2(tip.x, tip.y);
    }

    // -------------------------------------------------------
    // VALIDATE CUT
    // -------------------------------------------------------
    private void ValidateCut()
    {
        Collider2D col = currentItem.GetComponent<Collider2D>();
        itemCenter = col != null ? col.bounds.center : currentItem.transform.position;
        Vector2 tip = GetKnifeTip();

        if (!firstCutDone)
        {
            // FIRST CUT = VERTICAL → check X distance
            float distX = Mathf.Abs(tip.x - itemCenter.x);
            Debug.Log($"[QuarterCutter] Cut 1 (Vertical) | Tip X={tip.x} | Center X={itemCenter.x} | Dist={distX}");

            if (distX <= cutTolerance)
            {
                Debug.Log("[QuarterCutter] CORRECT first cut!");
                StartCoroutine(FirstCutSequence());
            }
            else
            {
                Debug.Log("[QuarterCutter] WRONG first cut!");
                StartCoroutine(WrongCutSequence());
            }
        }
        else
        {
            // SECOND CUT = HORIZONTAL → check Y distance
            float distY = Mathf.Abs(tip.y - itemCenter.y);
            Debug.Log($"[QuarterCutter] Cut 2 (Horizontal) | Tip Y={tip.y} | Center Y={itemCenter.y} | Dist={distY}");

            if (distY <= cutTolerance)
            {
                Debug.Log("[QuarterCutter] CORRECT second cut!");
                StartCoroutine(SecondCutSequence());
            }
            else
            {
                Debug.Log("[QuarterCutter] WRONG second cut!");
                StartCoroutine(WrongCutSequence());
            }
        }
    }

    // -------------------------------------------------------
    // FIRST CUT SEQUENCE (Vertical → Left + Right)
    // -------------------------------------------------------
    private IEnumerator FirstCutSequence()
    {
        yield return StartCoroutine(SlideKnifeVertical());

        Transform whole = currentItem.transform.Find("Whole");
        Transform left  = currentItem.transform.Find("Left");
        Transform right = currentItem.transform.Find("Right");
        Transform vLine = currentItem.transform.Find("DottedLineV");

        if (whole != null) whole.gameObject.SetActive(false);
        if (vLine != null) vLine.gameObject.SetActive(false);
        if (left != null)  left.gameObject.SetActive(true);
        if (right != null) right.gameObject.SetActive(true);

        if (left != null && right != null)
            yield return StartCoroutine(SeparateLeftRight(left, right));

        firstCutDone = true;

        yield return new WaitForSeconds(0.3f);
        knifeTransform.position = knifeStartPosition;
        knifeEnteredItem = false;

        // Rotate knife for horizontal cut — use localEulerAngles to avoid flip conflict
        knifeTransform.localEulerAngles = new Vector3(0, 0, 90f);

        levelController.OnFirstCutDone();
    }

    // -------------------------------------------------------
    // SECOND CUT SEQUENCE (Horizontal → 4 Quarters)
    // -------------------------------------------------------
    private IEnumerator SecondCutSequence()
    {
        secondCutDone = true;

        yield return StartCoroutine(SlideKnifeHorizontal());

        Transform left        = currentItem.transform.Find("Left");
        Transform right       = currentItem.transform.Find("Right");
        Transform topLeft     = currentItem.transform.Find("TopLeft");
        Transform topRight    = currentItem.transform.Find("TopRight");
        Transform bottomLeft  = currentItem.transform.Find("BottomLeft");
        Transform bottomRight = currentItem.transform.Find("BottomRight");
        Transform hLine       = currentItem.transform.Find("DottedLineH");

        if (left != null)  left.gameObject.SetActive(false);
        if (right != null) right.gameObject.SetActive(false);
        if (hLine != null) hLine.gameObject.SetActive(false);

        if (topLeft != null)     topLeft.gameObject.SetActive(true);
        if (topRight != null)    topRight.gameObject.SetActive(true);
        if (bottomLeft != null)  bottomLeft.gameObject.SetActive(true);
        if (bottomRight != null) bottomRight.gameObject.SetActive(true);

        if (topLeft != null && topRight != null && bottomLeft != null && bottomRight != null)
            yield return StartCoroutine(AnimateQuartersApart(topLeft, topRight, bottomLeft, bottomRight));

        yield return new WaitForSeconds(0.3f);
        knifeTransform.position = knifeStartPosition;
        knifeTransform.localEulerAngles = new Vector3(0, 0, 0);

        levelController.OnBothCutsDone();
    }

    // -------------------------------------------------------
    // WRONG CUT
    // -------------------------------------------------------
    private IEnumerator WrongCutSequence()
    {
        yield return StartCoroutine(ShakeItem());

        knifeTransform.position = knifeStartPosition;
        knifeEnteredItem = false;

        levelController.OnWrongCut();
    }

    // -------------------------------------------------------
    // KNIFE ANIMATIONS
    // -------------------------------------------------------
    private IEnumerator SlideKnifeHorizontal()
    {
        // Knife slides left to right across item
        Vector3 start = knifeTransform.position;
        Vector3 end   = new Vector3(itemCenter.x + 5f, knifeTransform.position.y, 0f);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            knifeTransform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }

    private IEnumerator SlideKnifeVertical()
    {
        // Knife slides top to bottom across item
        Vector3 start = knifeTransform.position;
        Vector3 end   = new Vector3(knifeTransform.position.x, itemCenter.y - 5f, 0f);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            knifeTransform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
    }

    // -------------------------------------------------------
    // SPLIT ANIMATIONS
    // -------------------------------------------------------
    private IEnumerator SeparateLeftRight(Transform left, Transform right)
    {
        Vector3 leftStart  = left.position;
        Vector3 rightStart = right.position;

        // Use item center to determine proper separation direction
        float centerX    = itemCenter.x;
        Vector3 leftEnd  = new Vector3(centerX - 1.2f, leftStart.y,  leftStart.z);
        Vector3 rightEnd = new Vector3(centerX + 1.2f, rightStart.y, rightStart.z);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            left.position  = Vector3.Lerp(leftStart,  leftEnd,  t);
            right.position = Vector3.Lerp(rightStart, rightEnd, t);
            yield return null;
        }
    }

    private IEnumerator AnimateQuartersApart(Transform tl, Transform tr, Transform bl, Transform br)
    {
        Vector3 tlStart = tl.position, trStart = tr.position;
        Vector3 blStart = bl.position, brStart = br.position;

        Vector3 tlEnd = tlStart + new Vector3(-0.6f,  0.6f, 0);
        Vector3 trEnd = trStart + new Vector3( 0.6f,  0.6f, 0);
        Vector3 blEnd = blStart + new Vector3(-0.6f, -0.6f, 0);
        Vector3 brEnd = brStart + new Vector3( 0.6f, -0.6f, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            tl.position = Vector3.Lerp(tlStart, tlEnd, t);
            tr.position = Vector3.Lerp(trStart, trEnd, t);
            bl.position = Vector3.Lerp(blStart, blEnd, t);
            br.position = Vector3.Lerp(brStart, brEnd, t);
            yield return null;
        }
    }

    private IEnumerator ShakeItem()
    {
        if (currentItem == null) yield break;

        Vector3 orig    = currentItem.transform.position;
        float   elapsed = 0f;

        while (elapsed < 0.5f)
        {
            float x = orig.x + Mathf.Sin(elapsed * 50f) * 0.15f;
            currentItem.transform.position = new Vector3(x, orig.y, orig.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentItem.transform.position = orig;
    }
}