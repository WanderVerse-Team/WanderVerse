using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "Scene_SignIn";

    private bool hasTransitioned;
    private bool videoCompleted;
    private bool videoFailed;

    void Start()
    {
        StartCoroutine(PlaySplashVideo());
    }

    private IEnumerator PlaySplashVideo()
    {
        if (videoPlayer == null)
        {
            Debug.LogError("[SplashController] VideoPlayer reference is missing. Skipping splash.");
            LoadNextScene();
            yield break;
        }

        // Rely on completion callback instead of length math (length can be 0/unknown on some devices).
        videoPlayer.playOnAwake = false;
        videoPlayer.loopPointReached += HandleVideoCompleted;
        videoPlayer.errorReceived += HandleVideoError;

        if (videoPlayer.clip == null && string.IsNullOrWhiteSpace(videoPlayer.url))
        {
            Debug.LogError("[SplashController] No video clip/url assigned. Skipping splash.");
            LoadNextScene();
            yield break;
        }

        videoPlayer.Prepare();

        const float prepareTimeoutSeconds = 8f;
        float prepareElapsed = 0f;

        while (!videoPlayer.isPrepared && !videoFailed && prepareElapsed < prepareTimeoutSeconds)
        {
            prepareElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!videoPlayer.isPrepared || videoFailed)
        {
            Debug.LogWarning("[SplashController] Video did not prepare in time. Continuing startup flow.");
            LoadNextScene();
            yield break;
        }

        videoPlayer.Play();

        float timeoutSeconds = videoPlayer.clip != null && videoPlayer.clip.length > 0
            ? (float)videoPlayer.clip.length + 2f
            : 10f;
        float elapsed = 0f;

        while (!videoCompleted && elapsed < timeoutSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (!videoCompleted)
        {
            Debug.LogWarning("[SplashController] Video completion callback timed out. Continuing startup flow.");
        }

        LoadNextScene();
    }

    private void HandleVideoCompleted(VideoPlayer source)
    {
        videoCompleted = true;
    }

    private void HandleVideoError(VideoPlayer source, string message)
    {
        Debug.LogError($"[SplashController] Video error: {message}");
        videoFailed = true;
    }

    private void LoadNextScene()
    {
        if (hasTransitioned)
        {
            return;
        }

        hasTransitioned = true;
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= HandleVideoCompleted;
            videoPlayer.errorReceived -= HandleVideoError;
        }

        Debug.Log("[SplashController] Splash complete. Loading next scene...");
        SceneManager.LoadScene(nextSceneName);
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= HandleVideoCompleted;
            videoPlayer.errorReceived -= HandleVideoError;
        }
    }
}
