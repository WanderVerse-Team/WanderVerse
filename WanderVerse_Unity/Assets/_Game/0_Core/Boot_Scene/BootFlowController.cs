using UnityEngine;
using UnityEngine.SceneManagement;

public class BootFlowController : MonoBehaviour
{
    private void OnEnable()
    {
        AppInitialization.OnInitializationComplete += TransitionToSignInScene;
    }

    private void OnDisable()
    {
        AppInitialization.OnInitializationComplete -= TransitionToSignInScene;
    }

    private void TransitionToSignInScene() 
    {
        Debug.Log("[BootFlowController] All systems green. Loading SignIn Scene...");
        SceneManager.LoadScene("Scene_SignIn");
    }
}
