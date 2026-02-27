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

        if (itemValue != null)
        {
            // 2. Tell the controller to add the points
            controller.ConsumeItem(itemValue.goldValue);

            // 3. Destroy the coin immediately
            Destroy(collision.gameObject);
        }
    }
   
}