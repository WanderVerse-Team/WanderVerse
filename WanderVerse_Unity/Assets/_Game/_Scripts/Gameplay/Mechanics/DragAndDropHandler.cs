using UnityEngine;

public class DragAndDropHandler : MonoBehaviour
{
    [Header("--- SETTINGS ---")]
    [Tooltip("Layer in which draggable objects exist")]
    [SerializeField] private LayerMask draggableLayer;

    [Tooltip("Layer in which target drop zones exist")]
    [SerializeField] private LayerMask dropZoneLayer;

    [Tooltip("How much to zoom the object when picking it up (for visual feedback).")]
    [SerializeField] private float dragScaleFactor = 1.2f;

    [Tooltip("Sorting order to apply when dragging so the selected item appears on top.")]
    [SerializeField] private int dragSortingOrder = 100;

    private Camera mainCamera;
    private GameObject selectedObject;
    private Vector3 offset;
    private Vector3 originalScale;
    private int originalSortingOrder;
    private SpriteRenderer selectedRenderer;
    private Rigidbody2D selectedRb;

    private BaseLevelController levelController;

    private void Start()
    {
        mainCamera = Camera.main;
        levelController = FindFirstObjectByType<BaseLevelController>();

        if (levelController == null)
        {
            Debug.LogError("[DragAndDropHandler] No LevelController found in scene!");
        }
    }

    private void Update()
    {
        // 1. INPUT: MOUSE DOWN (Pick Up)
        if (Input.GetMouseButtonDown(0))
        {
            AttemptPickUp();
        }

        // 2. INPUT: MOUSE DRAG (Move)
        if (selectedObject != null && Input.GetMouseButton(0))
        {
            DragObject();
        }

        // 3. INPUT: MOUSE UP (Drop)
        if (selectedObject != null && Input.GetMouseButtonUp(0))
        {
            DropObject();
        }
    }

    private void AttemptPickUp()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Raycast only against the Draggable Layer
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero, 0f, draggableLayer);

        if (hit.collider != null)
        {
            selectedObject = hit.collider.gameObject;

            // Store Original State
            originalScale = selectedObject.transform.localScale;
            selectedRenderer = selectedObject.GetComponent<SpriteRenderer>();
            selectedRb = selectedObject.GetComponent<Rigidbody2D>();

            // Calculate Offset (So object doesn't snap to center of mouse)
            offset = selectedObject.transform.position - (Vector3)mousePos;

            // Visual Feedback (Zoom effect & Bring to Front)
            selectedObject.transform.localScale = originalScale * dragScaleFactor;

            if (selectedRenderer != null)
            {
                originalSortingOrder = selectedRenderer.sortingOrder;
                selectedRenderer.sortingOrder = dragSortingOrder;
            }

            // Physics (Switch to Kinematic so it doesn't fall while holding)
            if (selectedRb != null)
            {
                selectedRb.bodyType = RigidbodyType2D.Kinematic;
                selectedRb.linearVelocity = Vector2.zero;
                selectedRb.angularVelocity = 0f;
            }
        }
    }

    private void DragObject()
    {
        Vector2 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        selectedObject.transform.position = (Vector3)mousePos + offset;
    }

    private void DropObject()
    {
        // Check if we dropped it onto a Drop Zone
        Vector2 dropPos = selectedObject.transform.position;
        RaycastHit2D hit = Physics2D.Raycast(dropPos, Vector2.zero, 0f, dropZoneLayer);

        // Reset Visuals
        selectedObject.transform.localScale = originalScale;
        if (selectedRenderer != null)
        {
            selectedRenderer.sortingOrder = originalSortingOrder;
        }

        // Reset Physics (Let gravity take over again)
        if (selectedRb != null)
        {
            selectedRb.bodyType = RigidbodyType2D.Dynamic;
        }

        // Validate the Drop
        if (hit.collider != null)
        {
            GameObject zone = hit.collider.gameObject;

            // Tell the Controller to handle the logic
            if (levelController != null)
            {
                levelController.ValidateDrop(selectedObject, zone);
            }
        }
        else
        {
            // Add Logic for dropping into empty space (Miss)
            // For now, physics takes over and it falls.
        }

        // Clear selection
        selectedObject = null;
        selectedRenderer = null;
        selectedRb = null;
    }
}
