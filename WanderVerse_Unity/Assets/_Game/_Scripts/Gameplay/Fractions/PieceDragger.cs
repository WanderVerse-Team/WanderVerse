using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

// Attach this to each quarter piece (TopLeft, TopRight, BottomLeft, BottomRight)
public class PieceDragger : MonoBehaviour
{
    [Header("--- References ---")]
    public FractionsLevelController levelController;
    public Transform plate;

    [Header("--- Settings ---")]
    public float snapDistance = 1.5f;

    // STATE
    private bool isDragging = false;
    private bool isPlaced   = false;
    private Vector3 resetPos; // Position after cut — reset here on wrong answer
    private Camera mainCam;

    private void Start()
    {
        mainCam  = Camera.main;
        resetPos = transform.position;
    }

    // Call this after cut animation completes to save the post-cut position
    public void SetResetPosition()
    {
        resetPos = transform.position;
    }

    private void Update()
    {
        if (isPlaced) return;

        var pointer = Pointer.current;
        if (pointer == null) return;

        Vector2 screenPos = pointer.position.ReadValue();
        Vector2 worldPos  = mainCam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, mainCam.nearClipPlane));

        // Press
        if (pointer.press.wasPressedThisFrame && !isDragging)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null && hit.transform == transform)
            {
                isDragging = true;
                Debug.Log($"[PieceDragger] Grabbed: {gameObject.name}");
            }
        }

        // Drag
        if (pointer.press.isPressed && isDragging)
        {
            transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
        }

        // Release
        if (pointer.press.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            CheckIfOnPlate();
        }
    }

    private void CheckIfOnPlate()
    {
        if (plate == null) return;

        float dist = Vector3.Distance(transform.position, plate.position);
        Debug.Log($"[PieceDragger] Released | Distance to plate: {dist}");

        if (dist <= snapDistance)
        {
            isPlaced = true;
            transform.position = plate.position + new Vector3(
                Random.Range(-0.3f, 0.3f),
                Random.Range(-0.3f, 0.3f),
                0f);

            Debug.Log($"[PieceDragger] Placed on plate!");

            if (levelController != null)
                levelController.OnPiecePlaced();

            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
        else
        {
            // Return to post-cut position
            transform.position = resetPos;
            Debug.Log($"[PieceDragger] Missed plate, returned to cut position.");
        }
    }

    // Called on wrong answer — animate back to post-cut position
    public void ResetPiece()
    {
        isPlaced   = false;
        isDragging = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;

        StartCoroutine(AnimateBackToReset());
    }

    private IEnumerator AnimateBackToReset()
    {
        Vector3 from = transform.position;
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            transform.position = Vector3.Lerp(from, resetPos, t);
            yield return null;
        }
        transform.position = resetPos;
    }
}