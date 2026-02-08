using UnityEngine;

public class DoorLogic : MonoBehaviour
{
    [Header("--- Background Images ---")]
    public SpriteRenderer bgrenderer;
    public Sprite doorClosedSprite;
    public Sprite doorOpenSprite;

    
    void Start()
    {
        if (bgrenderer != null && doorClosedSprite != null)
        {
            bgrenderer.sprite = doorClosedSprite;
        }
    }

    public void OpenDoor()
    {
        if (bgrenderer != null && doorOpenSprite != null)
        {
            bgrenderer.sprite = doorOpenSprite;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
