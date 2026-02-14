using UnityEngine;

public class DoorLipSync : MonoBehaviour
{
    [Header("--- Setup ---")]
    public AudioSource voiceSource;      // Drag the Audio Source here
    public SpriteRenderer doorRenderer;  // Drag the Door's Sprite Renderer here
    
    [Header("--- Sprites ---")]
    public Sprite mouthClosedSprite;     // The "Idle" face
    public Sprite mouthOpenSprite;       // The "Talking" face

    [Header("--- Settings ---")]
    [Tooltip("How loud the voice needs to be to open the mouth. Lower = More Sensitive.")]
    [Range(0.001f, 0.1f)]
    public float sensitivity = 0.01f; 

    void Update()
    {
        // 1. Safety Check
        if (voiceSource == null || doorRenderer == null) return;

        // 2. Is the audio playing?
        if (voiceSource.isPlaying)
        {
            // A. Get a small sample of the current audio spectrum (64 samples is enough)
            float[] samples = new float[64];
            voiceSource.GetOutputData(samples, 0);

            // B. Calculate the average volume (Amplitude)
            float sum = 0;
            foreach (float s in samples)
            {
                sum += Mathf.Abs(s);
            }
            float currentVolume = sum / 64;

            // C. Decide which sprite to show
            if (currentVolume > sensitivity)
            {
                doorRenderer.sprite = mouthOpenSprite;
            }
            else
            {
                doorRenderer.sprite = mouthClosedSprite;
            }
        }
        else
        {
            // 3. Not playing? Ensure mouth is closed.
            if (doorRenderer.sprite != mouthClosedSprite)
            {
                doorRenderer.sprite = mouthClosedSprite;
            }
        }
    }
}