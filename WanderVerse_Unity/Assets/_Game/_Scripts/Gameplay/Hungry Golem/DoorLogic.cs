using UnityEngine;

public class DoorLogic : MonoBehaviour
{
    [Header("--- Background Images ---")]
    public SpriteRenderer bgrenderer;
    public Sprite doorClosedSprite;
    public Sprite doorOpenSprite;
    

    
    void Start()
    {
        if (bgrenderer != null )
        {
            bgrenderer.sprite = doorClosedSprite;
        }
    }

    public void OpenDoor()
    {
        bgrenderer.sprite = doorOpenSprite;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
