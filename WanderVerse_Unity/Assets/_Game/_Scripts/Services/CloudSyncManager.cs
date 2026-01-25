using System;
using UnityEngine;
using UnityEngine.Networking;
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
    [SerializeField] private string syncEndpoint = "https://server-backend-eight.vercel.app/api/sync";

    [Serializable]
    public class SyncResponse 
    {
        public string status;       
        public string message;
        public PlayerData forceUpdate; 
    }

    public void SyncProgress(PlayerData localData)
    {
        StartCoroutine(SyncRoutine(localData));
    }

    private IEnumerator SyncRoutine(PlayerData data)
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) { Debug.LogError("Sync Failed: No user logged in."); yield break; }

        // 1. Get the Security Token from Firebase
        var tokenTask = user.TokenAsync(true);
        yield return new WaitUntil(() => tokenTask.IsCompleted);
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

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            SyncResponse response = JsonUtility.FromJson<SyncResponse>(request.downloadHandler.text);
            Debug.Log($"Sync Result: {response.status} - {response.message}");
            
            if (response.status == "CONFLICT") {
                Debug.LogWarning("Cloud data is better. Overwrite triggered.");
                // GameManager.Instance.UpdateFromCloud(response.forceUpdate);
            }
        }
        else
        {
            Debug.LogError($"Sync Error {request.responseCode}: {request.error}");
        }
    }
}