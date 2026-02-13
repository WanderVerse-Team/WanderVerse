using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("--- Audio Mixer ---")]
    [Tooltip("Drag the Master Audio Mixer here")]
    public AudioMixer mainMixer;

    [Header("--- Sources ---")]
    public AudioSource musicSource;
    public AudioSource sfxSource;   // For UI sounds (2D)
    public AudioSource voiceSource; // For Lesson Instructions

    [Header("--- Global UI Sounds ---")]
    public AudioClip clickSound;
    public AudioClip successSound;
    public AudioClip errorSound;
    public AudioClip levelCompleteSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Load Music Volume (Default to 0dB if not found)
        float savedMusicVol = PlayerPrefs.GetFloat("MusicVol", 0f);
        mainMixer.SetFloat("MusicVol", savedMusicVol);

        // Load SFX Volume
        float savedSFXVol = PlayerPrefs.GetFloat("SFXVol", 0f);
        mainMixer.SetFloat("SFXVol", savedSFXVol);

        // Load Master Volume
        float savedMasterVol = PlayerPrefs.GetFloat("MasterVol", 0f);
        mainMixer.SetFloat("MasterVol", savedMasterVol);
    }

    // ========================================================================
    // 1. MUSIC LOGIC (Smart Switching)
    // ========================================================================
    public void PlayMusic(AudioClip clip)
    {
        // If no clip is provided, or it's the same song, do nothing
        if (clip == null || musicSource.clip == clip) return;

        // Future polish: Add a Coroutine here to Fade Out -> Change Clip -> Fade In
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    // ========================================================================
    // 2. SFX LOGIC (Global UI)
    // ========================================================================
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) sfxSource.PlayOneShot(clip);
    }

    // Shortcuts for common UI sounds
    public void PlayClick() => PlaySFX(clickSound);
    public void PlaySuccess() => PlaySFX(successSound);
    public void PlayError() => PlaySFX(errorSound);
    public void PlayLevelComplete() => PlaySFX(levelCompleteSound);

    // ========================================================================
    // 3. VOICE LOGIC (Ducking Music)
    // ========================================================================
    public void PlayVoiceover(AudioClip clip)
    {
        if (clip == null) return;

        voiceSource.Stop(); // Stop any previous speech
        voiceSource.clip = clip;
        voiceSource.Play();

        // Optional: Lower music volume while voice plays
        StartCoroutine(DuckMusicRoutine(clip.length));
    }

    private IEnumerator DuckMusicRoutine(float duration)
    {
        // 1. Lower Music Volume instantly
        mainMixer.SetFloat("MusicVol", -20f); // Drop to -20dB

        // 2. Wait for voice to finish
        yield return new WaitForSeconds(duration);

        // 3. Restore Music Volume (assuming max is 0dB)
        mainMixer.SetFloat("MusicVol", 0f);
    }

    // ========================================================================
    // 4. VOLUME CONTROL (For Settings Menu)
    // ========================================================================
    // Call these from UI Sliders
    public void SetMasterVolume(float sliderValue) 
    {
        float volume = SliderValueToDB(sliderValue);

        mainMixer.SetFloat("MasterVol", volume);
        PlayerPrefs.SetFloat("MasterVol", volume);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float sliderValue) 
    {
        float volume = SliderValueToDB(sliderValue);

        mainMixer.SetFloat("MusicVol", volume);
        PlayerPrefs.SetFloat("MusicVol", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float sliderValue) 
    {
        float volume = SliderValueToDB(sliderValue);

        mainMixer.SetFloat("SFXVol", volume);
        PlayerPrefs.SetFloat("SFXVol", volume);
        PlayerPrefs.Save();
    }

    private float SliderValueToDB(float sliderValue) 
    {
        return Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20;
    }
}