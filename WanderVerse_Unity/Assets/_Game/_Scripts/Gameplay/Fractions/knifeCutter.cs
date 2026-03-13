using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class KnifeCutter : MonoBehaviour
{
    // -------------------------------------------------------
    // REFERENCES
    // -------------------------------------------------------
    [Header("--- Knife ---")]
    public Transform knifeTransform;
    private Vector3 knifeStartPosition;

    [Header("--- Cut Settings ---")]
    [Tooltip("How close to center counts as correct cut")]
    public float cutTolerance = 1.0f;

    [Tooltip("Offset from knife center to find the tip")]
    public float knifeTipOffset = 0.5f;

    [Header("--- Level Controller ---")]
    public FractionsLevelController levelController;

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    private GameObject currentItem;
    private Vector3 itemCenter;

    private bool isDragging = false;
    private bool isCutDone = false;
    private bool knifeEnteredItem = false;

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
    public void SetCurrentItem(GameObject item)
    {
        currentItem = item;
        itemCenter = item.transform.position;
        isCutDone = false;
        knifeEnteredItem = false;

        Debug.Log($"[KnifeCutter] Ready to cut: {item.name} | Center: {itemCenter}");
    }

    // -------------------------------------------------------
    // RESET
    // -------------------------------------------------------
    public void ResetKnife()
    {
        isDragging = false;
        isCutDone = false;
        knifeEnteredItem = false;
        knifeTransform.position = knifeStartPosition;

        Debug.Log("[KnifeCutter] Knife reset.");
    }

    // -------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------
    private void Update()
    {
        if (isCutDone) return;
        if (currentItem == null) return;

        // Get mouse position in world space
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane));

        // Press — check if player clicked the knife
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null && hit.transform == knifeTransform)
            {
                isDragging = true;
                Debug.Log("[KnifeCutter] Knife grabbed!");
            }
        }

        // Drag — move knife with finger
        if (Mouse.current.leftButton.isPressed && isDragging)
        {
            knifeTransform.position = new Vector3(worldPos.x, worldPos.y, 0f);
            CheckIfInsideItem();
        }

        // Release
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;

            if (knifeEnteredItem)
            {
                ValidateCut();
            }
            else
            {
                knifeTransform.position = knifeStartPosition;
                Debug.Log("[KnifeCutter] Knife released without touching item.");
            }
        }
    }

    // -------------------------------------------------------
    // CHECK IF KNIFE TIP IS INSIDE ITEM
    // -------------------------------------------------------
    private void CheckIfInsideItem()
    {
        if (currentItem == null) return;

        Collider2D itemCollider = currentItem.GetComponent<Collider2D>();
        if (itemCollider == null) return;

        // Get knife tip position
        // Tip is at the top of the knife sprite
        Vector2 knifeTip = GetKnifeTip();

        if (itemCollider.bounds.Contains(new Vector3(knifeTip.x, knifeTip.y, 0f)))
        {
            knifeEnteredItem = true;
            Debug.Log($"[KnifeCutter] Knife tip entered item at {knifeTip}");
        }
    }

    // Get the tip of the knife based on its rotation
    private Vector2 GetKnifeTip()
    {
        // Tip is offset in the knife's local UP direction
        Vector3 tip = knifeTransform.position + knifeTransform.up * knifeTipOffset;
        return new Vector2(tip.x, tip.y);
    }

    // -------------------------------------------------------
    // VALIDATE CUT
    // Check if knife tip X is close to item center X
    // -------------------------------------------------------
    private void ValidateCut()
    {
        isCutDone = true;

        // Always get fresh item center
        itemCenter = currentItem.transform.position;

        Vector2 knifeTip = GetKnifeTip();
        float distanceFromCenter = Mathf.Abs(knifeTip.x - itemCenter.x);

        Debug.Log($"[KnifeCutter] Knife tip X={knifeTip.x} | Center X={itemCenter.x} | Distance={distanceFromCenter} | Tolerance={cutTolerance}");

        if (distanceFromCenter <= cutTolerance)
        {
            Debug.Log("[KnifeCutter] CORRECT CUT!");
            StartCoroutine(CorrectCutSequence());
        }
        else
        {
            Debug.Log("[KnifeCutter] WRONG CUT!");
            StartCoroutine(WrongCutSequence());
        }
    }

    // -------------------------------------------------------
    // CORRECT CUT SEQUENCE
    // -------------------------------------------------------
    private IEnumerator CorrectCutSequence()
    {
        yield return StartCoroutine(SlideKnifeThrough());
        SplitItem();
        levelController.OnCorrectCut();

        // Reset knife back after cut
        yield return new WaitForSeconds(0.5f);
        knifeTransform.position = knifeStartPosition;
    }

    private IEnumerator SlideKnifeThrough()
    {
        // Slide knife horizontally through the item left to right
        Vector3 startPos = knifeTransform.position;
        Vector3 endPos = new Vector3(itemCenter.x - 5f, knifeTransform.position.y, 0f);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            knifeTransform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
    }

    private void SplitItem()
    {
        if (currentItem == null) return;

        Transform whole      = currentItem.transform.Find("Whole");
        Transform left       = currentItem.transform.Find("Left");
        Transform right      = currentItem.transform.Find("Right");
        Transform dottedLine = currentItem.transform.Find("DottedLine");

        if (whole != null)      whole.gameObject.SetActive(false);
        if (dottedLine != null) dottedLine.gameObject.SetActive(false);
        if (left != null)       left.gameObject.SetActive(true);
        if (right != null)      right.gameObject.SetActive(true);

        if (left != null && right != null)
            StartCoroutine(AnimateHalvesApart(left, right));
        else
            Debug.LogWarning($"[KnifeCutter] Missing Left or Right on {currentItem.name}!");
    }

    private IEnumerator AnimateHalvesApart(Transform left, Transform right)
    {
        Vector3 leftStart  = left.position;
        Vector3 rightStart = right.position;
        Vector3 leftEnd    = leftStart  + new Vector3(-0.7f, 0, 0);
        Vector3 rightEnd   = rightStart + new Vector3( 0.7f, 0, 0);

        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 2.5f;
            left.position  = Vector3.Lerp(leftStart,  leftEnd,  t);
            right.position = Vector3.Lerp(rightStart, rightEnd, t);
            yield return null;
        }
    }

    // -------------------------------------------------------
    // WRONG CUT SEQUENCE
    // -------------------------------------------------------
    private IEnumerator WrongCutSequence()
    {
        yield return StartCoroutine(ShakeItem());

        knifeTransform.position = knifeStartPosition;
        isCutDone = false;
        knifeEnteredItem = false;

        levelController.OnWrongCut();
    }

    private IEnumerator ShakeItem()
    {
        if (currentItem == null) yield break;

        Vector3 originalPos = currentItem.transform.position;
        float elapsed = 0f;

        while (elapsed < 0.5f)
        {
            float x = originalPos.x + Mathf.Sin(elapsed * 50f) * 0.15f;
            currentItem.transform.position = new Vector3(x, originalPos.y, originalPos.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentItem.transform.position = originalPos;
    }
}