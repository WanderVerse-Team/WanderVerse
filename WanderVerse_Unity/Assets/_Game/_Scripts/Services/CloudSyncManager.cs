using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Text;
using Firebase.Auth;
using WanderVerse.Framework.Data; 

[Serializable]
public class SyncPayload 
{
    public PlayerData localData;
}

public class CloudSyncManager : MonoBehaviour
{
    public static CloudSyncManager Instance { get; private set; }

    [SerializeField] private string syncEndpoint = "https://server-backend-eight.vercel.app/api/sync";

    [Serializable]
    public class SyncResponse 
    {
        public string status;       
        public string message;
        public PlayerData forceUpdate; 
    }

    private void Awake()
    {
        // Singleton Pattern setup
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

    public void SyncProgress(PlayerData localData)
    {
        StartCoroutine(SyncRoutine(localData));
    }

    private IEnumerator SyncRoutine(PlayerData data)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) { Debug.LogError("Sync Failed: No user logged in."); yield break; }
        
        // 1. Get the Security Token from Firebase (With 10s Timeout)
        var tokenTask = user.TokenAsync(true);
        float timeoutDuration = 10f;
        float timer = 0f;

        // Wait while the task is running, but stop if time runs out
        while (!tokenTask.IsCompleted)
        {
            timer += Time.deltaTime;
            if (timer > timeoutDuration)
            {
                Debug.LogError($"Sync Failed: Token Request Timed Out after {timeoutDuration} seconds.");
                yield break; // Stop the routine so the game doesn't hang
            }
            yield return null; // Wait for the next frame
        }

        if (tokenTask.IsFaulted || tokenTask.IsCanceled)
        {
             Debug.LogError("Sync Failed: Token Error - " + tokenTask.Exception);
             yield break;
        }

        string idToken = tokenTask.Result;
        
        // 2. Prepare Payload
        SyncPayload payload = new SyncPayload { localData = data };
        string jsonToSend = JsonUtility.ToJson(payload);

        // 3. Setup Request
        UnityWebRequest request = new UnityWebRequest(syncEndpoint, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonToSend);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        // 4. Attach Headers 
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + idToken); 

        // Add a timeout to the WebRequest itself too (Double Safety)
        request.timeout = 10; 

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            SyncResponse response = JsonUtility.FromJson<SyncResponse>(request.downloadHandler.text);
            Debug.Log($"Sync Result: {response.status} - {response.message}");
            
            if (response.status == "CONFLICT") {
                Debug.LogWarning("Cloud data is better. Overwrite triggered.");
                // TODO: Randiv needs to hook this up to GameManager
                // GameManager.Instance.UpdateFromCloud(response.forceUpdate);
            }
        }
        else
        {
            Debug.LogError($"Sync Error {request.responseCode}: {request.error}");
            Debug.LogError($"Server Message: {request.downloadHandler.text}");
        }
    }
}