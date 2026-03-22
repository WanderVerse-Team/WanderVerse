using UnityEngine;
using UnityEngine.EventSystems;

//IPointer interfaces provide better mobile support
public class DragDropItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    private Rigidbody2D rb;
    private Vector3 offset;
    private Camera mainCamera;

    public bool isReleased = false;
    

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main; //Cache the camera for performance
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //Calculate offset to prevent item from snapping to cursor
        Vector3 mousePos = GetMouseWorldPos(eventData.position);
        offset = transform.position - mousePos;

        //Disable gravity while dragging
        if (rb != null) rb.gravityScale = 0;
        //Clear any existing velocity
        rb.linearVelocity = Vector3.zero; 

        
        
        Debug.Log("Grabbed: " + gameObject.name);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //Update position based on mouse movement and offset
        transform.position = GetMouseWorldPos(eventData.position)+ offset;
    }

    
    public void OnPointerUp(PointerEventData eventData)
{
    if (rb != null) 
    {
        rb.gravityScale = 0.5f; //Re-enable gravity
        isReleased = true;      //Track that the item has been released
    }
}

//Handle collision with the Golem's mouth
private void OnTriggerEnter2D(Collider2D other)
{
    

    if (other.CompareTag("Mouth"))
    {
        var golem = Object.FindAnyObjectByType<HungryGolemController>();
        if (golem != null)
        {
            golem.ValidateDrop(this.gameObject, other.gameObject);
            Destroy(this.gameObject); //Destroy the fruit after validation
        }
    }
}

    //Convert screen coordinates to world space
    private Vector3 GetMouseWorldPos(Vector2 screenPosition)
    {
        Vector3 worldPoint = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 0));
        worldPoint.z = 0; // Keep it in 2D space
        return worldPoint;
    }
}