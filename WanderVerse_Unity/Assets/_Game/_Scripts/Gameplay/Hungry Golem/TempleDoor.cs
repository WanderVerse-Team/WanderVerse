using UnityEngine;
using UnityEngine.EventSystems;

public class TempleDoor : MonoBehaviour,IPointerClickHandler
{
    public HungryGolemController golem;
    private Rigidbody2D rb;
    private Camera mainCamera;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main; // Cache the camera for performance
    }
     
     
   

        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Temple Door Clicked");
            if (golem != null)
            {
                golem.CheckIfFinished();
            }
        }

        private void OnMouseEnter()
    {
        // Make the door glow or brighten
        GetComponent<SpriteRenderer>().color = Color.gray; 
    }

    private void OnMouseExit()
    {
        // Return to normal color
        GetComponent<SpriteRenderer>().color = Color.white;
    }

    
}
