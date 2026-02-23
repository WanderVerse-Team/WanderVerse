using System;
using UnityEngine;

public class TreasureItem : MonoBehaviour
{
    [Header("Item Value")]
    public int goldValue = 1; // Set to 10 for Gold Bars, 1 for Coins
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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

        // 1. Find the controller and the value script
        TreasurePackerController controller = FindObjectOfType<TreasurePackerController>();
        TreasureItem itemValue = GetComponent<TreasureItem>();

        // 3. Destroy as usual
        if (gameObject != null)
        {
            Destroy(gameObject);
        }
    }
}
