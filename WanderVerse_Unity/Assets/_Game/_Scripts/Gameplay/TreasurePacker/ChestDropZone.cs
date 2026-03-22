using UnityEngine;

public class ChestDropZone : MonoBehaviour
{
    [Header("References")]
    public TreasurePackerController controller;

    // This fires the exact frame another collider touches this object's collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Check if the object that touched the chest has a TreasureValue script
        TreasureItem itemValue = collision.GetComponent<TreasureItem>();

        if (itemValue != null && itemValue.hasBeenCounted == false)
        {
            itemValue.hasBeenCounted = true;
            collision.enabled = false; // Disable the collider to prevent multiple triggers
            // Add the points in the controller
            controller.ConsumeItem(itemValue.goldValue);

            // 3. Destroy the coin immediately
            Destroy(collision.gameObject);
        }
    }
   
}