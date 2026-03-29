using UnityEngine;
using UnityEngine.SceneManagement;

public class BootFlowController : MonoBehaviour
{
    [SerializeField] private string splashSceneName = "Scene_Splash";

    private void OnEnable()
    {
        AppInitialization.OnInitializationComplete += TransitionToSplashScreen;
    }

    private void OnDisable()
    {
        AppInitialization.OnInitializationComplete -= TransitionToSplashScreen;
    }

    private void TransitionToSplashScreen() 
    {
        Debug.Log("[BootFlowController] All systems green. Loading Splash Screen...");
        SceneManager.LoadScene(splashSceneName);
    }
}
