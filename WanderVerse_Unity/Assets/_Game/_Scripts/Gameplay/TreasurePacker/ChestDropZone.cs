using UnityEngine;
using UnityEngine.EventSystems;

public class ChestDropZone : MonoBehaviour, IDropHandler
{
    [Header("References")]
    public TreasurePackerController controller; // Drag your Game Controller here in the Inspector

    public void OnDrop(PointerEventData eventData)
    {
        // 1. Get the exact object that was dropped on the chest
        GameObject droppedItem = eventData.pointerDrag;

        if (droppedItem != null)
        {
            // 2. Read its value using the small script we made above
            TreasureItem itemValue = droppedItem.GetComponent<TreasureItem>();

            if (itemValue != null)
            {
                // 3. Send that value to your main game controller
                controller.AddGold(itemValue.goldValue);
            }

            // 4. Destroy the dragged item so it "disappears" into the chest
            Destroy(droppedItem);
        }
    }
}