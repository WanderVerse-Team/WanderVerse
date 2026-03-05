using UnityEngine;

public class DoorLipSync : MonoBehaviour
{
    [Header("--- Setup ---")]
    public AudioSource voiceSource;      // Drag the Door's AudioSource here
    public SpriteRenderer doorRenderer;  // Drag the Door's SpriteRenderer here
    
    [Header("--- Sprites ---")]
    public Sprite mouthClosedSprite;     // Face when silent
    public Sprite mouthOpenSprite;       // Face when talking

    void Update()
    {
        // Safety check to prevent errors
        if (voiceSource == null || doorRenderer == null) return;

        // Check if the audio is playing
        if (voiceSource.isPlaying)
        {
            doorRenderer.sprite = mouthOpenSprite;
        }
        else
        {
            doorRenderer.sprite = mouthClosedSprite;
        }
    }
}