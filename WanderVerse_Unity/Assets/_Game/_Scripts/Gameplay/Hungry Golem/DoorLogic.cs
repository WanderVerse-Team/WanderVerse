using UnityEngine;

public class DoorLogic : MonoBehaviour
{
    [Header("--- Background Images ---")]
    public SpriteRenderer bgrenderer;
    public Sprite doorClosedSprite;
    public Sprite doorOpenSprite;
    // public Sprite doorClosedSprite_720;
    // public Sprite doorClosedSprite_1080;
    // public Sprite doorClosedSprite_1440;
    // public Sprite doorOpenSprite_720;
    // public Sprite doorOpenSprite_1080;
    // public Sprite doorOpenSprite_1440;

    
    void Start()
    {
        if (bgrenderer != null )
        {
            bgrenderer.sprite = doorClosedSprite;
        //     if (Screen.height <= 720)
        // {
        //     bgrenderer.sprite = doorClosedSprite_720;
        // }
        // else if (Screen.height <= 1080)
        // {
        //     bgrenderer.sprite = doorClosedSprite_1080;
        // }
        // else
        // {
        //     bgrenderer.sprite = doorClosedSprite_1440;
        // }
        }
    }

    public void OpenDoor()
    {
        bgrenderer.sprite = doorOpenSprite;
        // if (bgrenderer != null)
        // {
        //     if (Screen.height <= 720)
        //     {
        //         bgrenderer.sprite = doorOpenSprite_720;
        //     }
        //     else if (Screen.height <= 1080)
        //     {
        //         bgrenderer.sprite = doorOpenSprite_1080;
        //     }
        //     else
        //     {
        //         bgrenderer.sprite = doorOpenSprite_1440;
        //     }
        // }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
