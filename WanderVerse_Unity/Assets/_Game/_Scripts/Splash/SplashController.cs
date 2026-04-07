using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string nextSceneName = "Scene_SignIn";

    void Start()
    {
        StartCoroutine(PlaySplashVideo());
    }

    private IEnumerator PlaySplashVideo()
    {
        videoPlayer.Prepare();

        while (!videoPlayer.isPrepared)
        {
            yield return null;
        }

        videoPlayer.Play();

        float waitTime = (float)videoPlayer.length - 0.1f;
        yield return new WaitForSeconds(waitTime);

        Debug.Log("[SplashController] Splash video complete. Loading Sign In...");
        SceneManager.LoadScene(nextSceneName);
    }
}