using UnityEngine;
using UnityEngine.InputSystem;

public class WorldMapScroll : MonoBehaviour
{
    [Header("References")]
    public Transform scrollRoot;     // Assign ScrollRoot
    public Camera mainCamera;        // Assign Main Camera (optional for later)

    [Header("Scroll Settings")]
    public float scrollSpeed = 0.02f;  // Tune this (smaller = slower)
    public float minY = -20f;          // Bottom limit
    public float maxY = 0f;            // Top limit

    private bool isDragging = false;
    private Vector2 lastScreenPos;

    void Update()
    {
        var mouse = Mouse.current;
        var touch = Touchscreen.current;

        // ---- Touch (mobile) ----
        if (touch != null && touch.primaryTouch.press.isPressed)
        {
            Vector2 pos = touch.primaryTouch.position.ReadValue();

            if (!isDragging)
            {
                isDragging = true;
                lastScreenPos = pos;
                return;
            }

            Drag(pos);
            return;
        }

        // Touch released
        if (isDragging && touch != null && !touch.primaryTouch.press.isPressed)
        {
            isDragging = false;
        }

        // ---- Mouse (PC) ----
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastScreenPos = mouse.position.ReadValue();
        }

        if (isDragging && mouse != null && mouse.leftButton.isPressed)
        {
            Drag(mouse.position.ReadValue());
        }

        if (mouse != null && mouse.leftButton.wasReleasedThisFrame)
        {
            isDragging = false;
        }
    }

    private void Drag(Vector2 currentScreenPos)
    {
        Vector2 delta = currentScreenPos - lastScreenPos;

        // Move ScrollRoot in world space based on screen drag
        float moveY = delta.y * scrollSpeed; // positive drag up moves map up
        Vector3 newPos = scrollRoot.position + new Vector3(0f, moveY, 0f);

        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);
        scrollRoot.position = newPos;

        lastScreenPos = currentScreenPos;
    }
}