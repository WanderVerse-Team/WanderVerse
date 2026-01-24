using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

//Reference to PlayerData and util classes
using WanderVerse.Framework.Data;
using WanderVerse.Framework.Utilities;

using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace wanderVerse.Backend
{
    public class LocalDataManager : MonoBehaviour
    {
        public static LocalDataManager Instance;

        //File name for saving data
        private readonly string saveFileName = "wanderverse_save.dat";

        //These keys stays empty until the user log in get them from the server
        private string _runtimeKey="";
        private string _runtimeIV="";

        public bool IsSecurityInitialized => !string.IsNullOrEmpty(_runtimeKey);

        private void Awake()
        {
            if (Instance==null)
            {
                Instance = this; 
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        //This is called once the player logs in and the server sends the keys
        public void InitializeSecurity(string key, string iv)
        {
            _runtimeKey = key;
            _runtimeIV = iv;
            Debug.Log("LocalDataManager: Security initialized.");
        }

        //Saves the player progress to the device after encrypting it
        public void SaveGame(PlayerData data)
        {
            if (!IsSecurityInitialized)
            {
                Debug.LogError("LocalDataManager: Security not initialized. Cannot save game. Are you logged in?");
                return;
            }

            try
            {   
                //Turn the data class into a JSON string
                string json = JsonUtility.ToJson(data);

                //Encrypt the JSON string
                byte[] encrypted = EncryptionUtils.Encrypt(json, _runtimeKey, _runtimeIV);

                //Save the encrypted data to the device
                string path = Path.Combine(Application.persistentDataPath, saveFileName);
                File.WriteAllBytes(path, encrypted);

                Debug.Log($"[LocalDataManager] Progress saved securely at {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalDataManager] Save Error: {e.Message}");
            }
        }

        //Loads the player data from the device after decrypting it
        public PlayerData LoadGame()
        {
            if (!IsSecurityInitialized)
            {
                Debug.LogError("[LocalDataManager] Load failed: keys not initialized.");
                return null;
            }

            string path = Path.Combine(Application.persistentDataPath, saveFileName);

            if (!File.Exists(path)) return null;

            try
            {
                //Read the encrypted data from the file
                byte[] encrypted = File.ReadAllBytes(path);

                //Use the utility to decrypt the data
                string json = EncryptionUtils.Decrypt(encrypted, _runtimeKey, _runtimeIV);

                //Turn the JSON string back into the player data object
                return JsonUtility.FromJson<PlayerData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalDataManager] Load Failed. Files maybe corrupted or keys changed: {e.Message}");
                return null;
            }

        }
    }
}