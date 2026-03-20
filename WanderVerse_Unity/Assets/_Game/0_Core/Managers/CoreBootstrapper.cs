using UnityEngine;
using System.Collections.Generic;

public static class CoreBootstrapper
{
    // Tell Unity to run this exactly once, right before the first scene loads
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void InitializeCoreSystems() 
    {
        string[] systemPrefabs =
        {
            "GameManager",
            "AudioManager",
            "CloudSyncManager",
            "LocalDataManager",
            "EnergyManager"
        };

        // Create a list to track the prefabs that actually spawned for debugging
        List<string> successfullySpawned = new List<string>();

        foreach (string prefabName in systemPrefabs) 
        {
            // Check if the manager already exists in the hierarchy
            if (GameObject.Find(prefabName) != null) continue;

            // Load the prefab directly from the Resources folder
            GameObject prefab = Resources.Load<GameObject>(prefabName);

            if (prefab == null) 
            {
                Debug.LogWarning($"[CoreBootstrapper] '{prefabName}' prefab not found! Make sure it is in Assets/_Game/0_Core/Resources/.");
                continue;
            }

            // Spawn the prefab into the game as a ROOT object
            GameObject instance = Object.Instantiate(prefab);

            // Remove the "(Clone)" text from the hierarchy
            instance.name = prefabName;

            successfullySpawned.Add(prefabName);
        }

        // Print the names of the spawned prefabs in the console for debugging
        if (successfullySpawned.Count > 0) 
        {
            string spawnedNames = string.Join(", ", successfullySpawned);
            Debug.Log($"[CoreBootstrapper] Sucessfully spawned: {spawnedNames}. Scripts will handle their own persistence.");
        }
    }
}
