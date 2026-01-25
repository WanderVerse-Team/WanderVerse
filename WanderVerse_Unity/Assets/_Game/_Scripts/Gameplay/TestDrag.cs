using UnityEngine;
using UnityEngine.EventSystems;

// We keep your IPointer interfaces because they are great for mobile!
public class TestDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Rigidbody2D rb;
    private Vector3 offset;
    private Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main; // Cache the camera for performance
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // 1. Calculate offset so the fruit doesn't "snap" its center to the mouse
        Vector3 mousePos = GetMouseWorldPos(eventData.position);
        offset = transform.position - mousePos;

        // 2. Disable gravity so it stays in your "hand"
        if (rb != null) rb.gravityScale = 0;
        
        Debug.Log("Grabbed: " + gameObject.name);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 3. Move the object to the mouse position + the original offset
        transform.position = GetMouseWorldPos(eventData.position) + offset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 4. Re-enable gravity so the fruit can fall into the Golem's mouth
        if (rb != null) rb.gravityScale = 0.5f;

        Debug.Log("Released: " + gameObject.name);
    }

    // Helper function to turn screen pixels into game world coordinates
    private Vector3 GetMouseWorldPos(Vector2 screenPosition)
    {
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        worldPoint.z = 0; // Keep it in 2D space
        return worldPoint;
    }
}