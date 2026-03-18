using UnityEngine;
using UnityEngine.InputSystem;

// Attach this to each quarter piece (TopLeft, TopRight, BottomLeft, BottomRight)
public class PieceDragger : MonoBehaviour
{
    [Header("--- References ---")]
    public FractionsLevelController levelController;
    public Transform plate;

    [Header("--- Settings ---")]
    [Tooltip("How close to plate center to count as placed")]
    public float snapDistance = 1.5f;

    // -------------------------------------------------------
    // STATE
    // -------------------------------------------------------
    private bool isDragging   = false;
    private bool isPlaced     = false;
    private Vector3 startPos;
    private Camera mainCam;

    // -------------------------------------------------------
    // START
    // -------------------------------------------------------
    private void Start()
    {
        startPos  = transform.position;
        mainCam   = Camera.main;
    }

    // -------------------------------------------------------
    // UPDATE
    // -------------------------------------------------------
    private void Update()
    {
        if (isPlaced) return;

        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos  = mainCam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, mainCam.nearClipPlane));

        // Press — check if player clicked this piece
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Collider2D hit = Physics2D.OverlapPoint(worldPos);
            if (hit != null && hit.transform == transform)
            {
                isDragging = true;
                Debug.Log($"[PieceDragger] Grabbed: {gameObject.name}");
            }
        }

        // Drag
        if (Mouse.current.leftButton.isPressed && isDragging)
        {
            transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
        }

        // Release
        if (Mouse.current.leftButton.wasReleasedThisFrame && isDragging)
        {
            isDragging = false;
            CheckIfOnPlate();
        }
    }

    // -------------------------------------------------------
    // CHECK IF PLACED ON PLATE
    // -------------------------------------------------------
    private void CheckIfOnPlate()
    {
        if (plate == null) return;

        float dist = Vector3.Distance(transform.position, plate.position);
        Debug.Log($"[PieceDragger] Released | Distance to plate: {dist}");

        if (dist <= snapDistance)
        {
            // Snap to plate
            isPlaced = true;
            transform.position = plate.position + new Vector3(
                Random.Range(-0.3f, 0.3f), 
                Random.Range(-0.3f, 0.3f), 
                0f);

            Debug.Log($"[PieceDragger] Placed on plate!");

            if (levelController != null)
                levelController.OnPiecePlaced();

            // Disable collider so it can't be moved again
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
        else
        {
            // Return to start position
            transform.position = startPos;
            Debug.Log($"[PieceDragger] Missed plate, returned to start.");
        }
    }

    // -------------------------------------------------------
    // RESET (called when new item starts)
    // -------------------------------------------------------
    public void ResetPiece()
    {
        isPlaced   = false;
        isDragging = false;
        transform.position = startPos;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = true;
    }
}
