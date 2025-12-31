using UnityEngine;
using TMPro;                // For TextMeshPro Inputs
using Firebase.Auth;        // For Firebase
using UnityEngine.SceneManagement; 

public class AuthManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TextMeshProUGUI feedbackText; // To show "Wrong Password" etc.

    private FirebaseAuth _auth;

    void Start()
    {
        _auth = FirebaseAuth.DefaultInstance;
        if(feedbackText != null) feedbackText.text = "";
    }

    // --- BUTTON FUNCTIONS ---

    public void OnSignUpButton()
    {
        StartCoroutine(RegisterUser(emailInput.text, passwordInput.text));
    }

    public void OnLoginButton()
    {
        StartCoroutine(LoginUser(emailInput.text, passwordInput.text));
    }

    // --- LOGIC ---

    private System.Collections.IEnumerator RegisterUser(string email, string password)
    {
        var task = _auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Register Failed: {task.Exception.InnerException?.Message}");
            UpdateFeedback($"Error: {task.Exception.InnerException?.Message}");
        }
        else
        {
            Debug.Log("User Created!");
            UpdateFeedback("Account Created! Logging in...");
        }
    }

    private System.Collections.IEnumerator LoginUser(string email, string password)
    {
        var task = _auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            Debug.LogError($"Login Failed: {task.Exception.InnerException?.Message}");
            UpdateFeedback("Login Failed. Check email/password.");
        }
        else
        {
            Debug.Log($"Logged In: {_auth.CurrentUser.Email}");
            UpdateFeedback("Success! Loading Menu...");
            
            SceneManager.LoadScene("Scene_WorldMap");
        }
    }

    private void UpdateFeedback(string message)
    {
        if (feedbackText != null) feedbackText.text = message;
    }
}