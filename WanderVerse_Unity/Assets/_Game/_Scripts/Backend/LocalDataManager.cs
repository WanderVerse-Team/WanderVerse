using UnityEngine;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

//Reference to PlayerData and util classes
using WanderVerse.Framework.Data;
using WanderVerse.Framework.Utilities;

namespace WanderVerse.Backend
{
    public class LocalDataManager : MonoBehaviour
    {
        public static LocalDataManager Instance;

        //File name for saving data
        private readonly string saveFileName = "wanderverse_save.dat";
        private readonly string guestSaveFileName = "wanderverse_guest_save.dat";

        //These keys stays empty until the user log in get them from the server
        private string _runtimeKey="";
        private string _runtimeIV="";

        //Guest mode flag - true when playing without authentication
        private bool _isGuestMode = false;
        public bool IsGuestMode => _isGuestMode;

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
            _isGuestMode = false; //Disable guest mode when authenticated
            Debug.Log("LocalDataManager: Security initialized.");
        }

        //Enable guest mode for playing without authentication
        public void EnableGuestMode()
        {
            _isGuestMode = true;
            _runtimeKey = "";
            _runtimeIV = "";
            Debug.Log("LocalDataManager: Guest mode enabled.");
        }

        //Saves the player progress to the device after encrypting it
        public void SaveGame(PlayerData data)
        {
            //Guest mode: save unencrypted JSON
            if (_isGuestMode)
            {
                SaveGuestGame(data);
                return;
            }

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
            //Guest mode: load unencrypted JSON
            if (_isGuestMode)
            {
                return LoadGuestGame();
            }

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
        public bool HasSaveFile()
        {
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            return File.Exists(path);
        }

        public void DeleteSaveFile()
        {
            string path = Path.Combine(Application.persistentDataPath, saveFileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("[LocalDataManager] Save file deleted.");
    
            }
        }

        //=== GUEST MODE METHODS ===

        //Saves guest data as plain JSON (not encrypted)
        private void SaveGuestGame(PlayerData data)
        {
            try
            {
                string json = JsonUtility.ToJson(data);
                string path = Path.Combine(Application.persistentDataPath, guestSaveFileName);
                File.WriteAllText(path, json);
                Debug.Log($"[LocalDataManager] Guest progress saved at {path}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalDataManager] Guest Save Error: {e.Message}");
            }
        }

        //Loads guest data from plain JSON
        private PlayerData LoadGuestGame()
        {
            string path = Path.Combine(Application.persistentDataPath, guestSaveFileName);

            if (!File.Exists(path)) return null;

            try
            {
                string json = File.ReadAllText(path);
                return JsonUtility.FromJson<PlayerData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalDataManager] Guest Load Failed: {e.Message}");
                return null;
            }
        }

        //Check if guest save exists (for migration prompt)
        public bool HasGuestSaveFile()
        {
            string path = Path.Combine(Application.persistentDataPath, guestSaveFileName);
            return File.Exists(path);
        }

        //Migrates guest data to authenticated account
        //Call this after user creates account and InitializeSecurity is called
        public bool MigrateGuestDataToAccount()
        {
            if (!IsSecurityInitialized)
            {
                Debug.LogError("[LocalDataManager] Cannot migrate: security not initialized.");
                return false;
            }

            string guestPath = Path.Combine(Application.persistentDataPath, guestSaveFileName);
            if (!File.Exists(guestPath))
            {
                Debug.LogWarning("[LocalDataManager] No guest data to migrate.");
                return false;
            }

            try
            {
                //Load guest data
                string json = File.ReadAllText(guestPath);
                PlayerData guestData = JsonUtility.FromJson<PlayerData>(json);

                //Save as encrypted authenticated data
                SaveGame(guestData);

                //Delete guest save file after successful migration
                File.Delete(guestPath);
                Debug.Log("[LocalDataManager] Guest data migrated successfully.");
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[LocalDataManager] Migration failed: {e.Message}");
                return false;
            }
        }

        //Deletes guest save file
        public void DeleteGuestSaveFile()
        {
            string path = Path.Combine(Application.persistentDataPath, guestSaveFileName);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log("[LocalDataManager] Guest save file deleted.");
            }
        }
    }
}