using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class KnifeCutter : MonoBehaviour
{
    [Header("--- Knife ---")]
    public Transform knifeTransform;
    private Vector3 knifeStartPosition;

    [Header("--- Cut Settings ---")]
    public float cutTolerance = 1.0f;
    public float knifeTipOffset = 0.5f;

    [Header("--- Level Controller ---")]
    public FractionsLevelController levelController;

    // STATE
    private GameObject currentItem;
    private Vector3 itemCenter;
    private bool isGuided;

    private bool isDragging       = false;
    private bool knifeEnteredItem = false;
    private bool cutDone          = false;

    private void Start()
    {
        knifeStartPosition = knifeTransform.position;
    }

    public void SetCurrentItem(GameObject item, bool guided)
    {
        currentItem      = item;
        isGuided         = guided;
        cutDone          = false;
        knifeEnteredItem = false;

        // Use collider bounds center — fixes offset sprites!
        Collider2D col = item.GetComponent<Collider2D>();
        itemCenter = col != null ? col.bounds.center : item.transform.position;

        Debug.Log($"[KnifeCutter] Ready: {item.name} | Center: {itemCenter} | Guided: {guided}");
    }

    public void ResetKnife()
    {
        isDragging       = false;
        cutDone          = false;
        knifeEnteredItem = false;
        knifeTransform.position = knifeStartPosition;
    }

    private void Update()
    {
        if (cutDone) return;
        if (currentItem == null) return;

        // Use Pointer — works for both mouse and touch automatically
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
                Debug.Log("[KnifeCutter] Knife grabbed!");
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
                Debug.Log("[KnifeCutter] Knife released without touching item.");
            }
        }
    }

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

    private void ValidateCut()
    {
        // Refresh center using collider bounds
        Collider2D col = currentItem.GetComponent<Collider2D>();
        itemCenter = col != null ? col.bounds.center : currentItem.transform.position;

        Vector2 tip   = GetKnifeTip();
        float   distX = Mathf.Abs(tip.x - itemCenter.x);

        Debug.Log($"[KnifeCutter] Tip X={tip.x} | Center X={itemCenter.x} | Dist={distX} | Tolerance={cutTolerance}");

        if (distX <= cutTolerance)
        {
            cutDone = true;
            Debug.Log("[KnifeCutter] CORRECT cut!");
            StartCoroutine(CorrectCutSequence());
        }
        else
        {
            Debug.Log("[KnifeCutter] WRONG cut!");
            StartCoroutine(WrongCutSequence());
        }
    }

    private IEnumerator CorrectCutSequence()
    {
        yield return StartCoroutine(SlideKnifeThrough());

        Transform whole = currentItem.transform.Find("Whole");
        Transform left  = currentItem.transform.Find("Left");
        Transform right = currentItem.transform.Find("Right");
        Transform dLine = currentItem.transform.Find("DottedLine");

        if (whole != null) whole.gameObject.SetActive(false);
        if (dLine != null) dLine.gameObject.SetActive(false);
        if (left  != null) left.gameObject.SetActive(true);
        if (right != null) right.gameObject.SetActive(true);

        if (left != null && right != null)
            yield return StartCoroutine(SeparateHalves(left, right));

        levelController.OnCorrectCut();
    }

    private IEnumerator WrongCutSequence()
    {
        yield return StartCoroutine(ShakeItem());
        knifeTransform.position = knifeStartPosition;
        knifeEnteredItem = false;
        levelController.OnWrongCut();
    }

    private IEnumerator SlideKnifeThrough()
    {
        Vector3 start = knifeTransform.position;
        Vector3 end   = new Vector3(itemCenter.x, itemCenter.y - 5f, 0f);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 12f;
            knifeTransform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        // Reset immediately to start position
        knifeTransform.position = knifeStartPosition;
    }

    private IEnumerator SeparateHalves(Transform left, Transform right)
    {
        Vector3 leftStart  = left.position;
        Vector3 rightStart = right.position;
        Vector3 leftEnd    = leftStart  + new Vector3(-0.5f, 0, 0);
        Vector3 rightEnd   = rightStart + new Vector3( 0.5f, 0, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            left.position  = Vector3.Lerp(leftStart,  leftEnd,  t);
            right.position = Vector3.Lerp(rightStart, rightEnd, t);
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