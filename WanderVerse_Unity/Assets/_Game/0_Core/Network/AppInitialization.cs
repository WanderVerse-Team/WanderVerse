using UnityEngine;
using Firebase;
using Firebase.Extensions;
using System.Threading.Tasks;

public class AppInitialization : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Starting Firebase Initialization...");

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                // Firebase is ready!
                Debug.Log("Firebase is ready! Proceeding to game...");
            }
            else
            {
                Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });
    }
}