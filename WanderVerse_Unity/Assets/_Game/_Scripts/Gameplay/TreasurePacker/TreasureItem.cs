using System;
using UnityEngine;

public class TreasureItem : MonoBehaviour
{
    [Header("Item Value")]
    public int goldValue = 1; 

    
    [HideInInspector] 
    public bool hasBeenCounted = false;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //To delete an item if it goes out of frame
   void OnBecameInvisible()
    {
        if (!Application.isPlaying) return;

        // Find the controller and the value script
        TreasurePackerController controller = FindObjectOfType<TreasurePackerController>();
        TreasureItem itemValue = GetComponent<TreasureItem>();

        // Destroy as usual
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
