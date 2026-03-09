using UnityEngine;

public class PlannerDropZone : MonoBehaviour
{
    [Tooltip("Which day belongs in this slot? (e.g., 'Monday')")]
    public string expectedDay;

    public DaysSequenceController levelController;

    private bool isFilled = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isFilled) return;

        DayTileIdentity dayTile = collision.GetComponent<DayTileIdentity>();
        DragDropItem dragItem = collision.GetComponent<DragDropItem>();

        // If it's a Day Tile and the player let go...
        if (dayTile != null && dragItem != null && dragItem.isReleased)
        {
            // Did they drop it in the correct order?
            if (dayTile.dayName.ToUpper() == expectedDay.ToUpper())
            {
                LockDayInPlace(dayTile, dragItem);
            }
            else
            {
                // WRONG SLOT! 
                Debug.Log($"<color=red>Wrong Order!</color> Expected {expectedDay}, got {dayTile.dayName}");
                levelController.DayPlacedWrong();
                
                // Reset the tile so the player has to try again
                dragItem.isReleased = false; 
            }
        }
    }

    private void LockDayInPlace(DayTileIdentity dayTile, DragDropItem dragItem)
    {
        isFilled = true;
        
        // Snap the day tile perfectly into the planner slot
        dayTile.transform.position = transform.position;

        // Destroy the drag script so they can't remove it
        Destroy(dragItem);

        if (AudioManager.Instance != null) AudioManager.Instance.PlaySuccess();
        Debug.Log($"<color=green>Correct!</color> {dayTile.dayName} placed successfully!");

        // Tell the main controller to add a point!
        levelController.DayPlacedCorrectly();
    }
}