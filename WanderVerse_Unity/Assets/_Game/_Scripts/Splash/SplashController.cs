using UnityEngine;
using UnityEngine.SceneManagement;

public class SplashController : MonoBehaviour
{
    [SerializeField] private Animator laserAnimator; 
    [SerializeField] private float delayBeforeSwitch = 3.0f;

    void Start()
    {
        if (laserAnimator != null)
        {
            laserAnimator.Play("Laser_Sweep_Anim"); 
        }

        Invoke("SwitchToSignIn", delayBeforeSwitch);
    }

    void SwitchToSignIn()
    {
        SceneManager.LoadScene("Scene_SignIn");
    }
}