using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A single cell in the vertical-addition grid.
/// When a battery is dropped onto it, it snaps in and notifies the controller.
/// Tapping an occupied socket removes the battery back to the tray.
/// </summary>
public class BatterySocket : MonoBehaviour, IDropHandler, IPointerClickHandler
{
    [Header("--- Socket Identity ---")]
    [Tooltip("Column index: 0 = leftmost (highest place value)")]
    public int column;

    [Tooltip("Row index: which addend row (0, 1, …)")]
    public int row;

    [HideInInspector] public BatteryIdentity currentBattery;

    private PowerStationController controller;

    private void Start()
    {
        controller = FindAnyObjectByType<PowerStationController>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        BatteryDragDrop drag = dropped.GetComponent<BatteryDragDrop>();
        BatteryIdentity id  = dropped.GetComponent<BatteryIdentity>();
        if (drag == null || id == null) return;

        TryAcceptBattery(drag, id);
    }

    public bool TryAcceptBattery(BatteryDragDrop drag, BatteryIdentity id)
    {
        // Socket already occupied
        if (currentBattery != null) return false;
        if (drag == null || id == null) return false;

        if (controller == null)
            controller = FindAnyObjectByType<PowerStationController>();

        // Snap battery into this socket
        currentBattery = id;
        drag.LockIntoSocket(transform);

        Debug.Log($"Battery {id.digitValue} placed at Row {row}, Col {column}");

        // Notify the controller (updates live column sums)
        if (controller != null)
            controller.OnBatteryPlaced();

        return true;
    }

    /// <summary>Tapping an occupied socket removes the battery and sends it back to the tray.</summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentBattery == null) return;

        // Return battery to the tray
        Transform tray = controller.batteryTray;
        BatteryDragDrop drag = currentBattery.GetComponent<BatteryDragDrop>();
        if (drag != null) drag.Unlock(tray);

        currentBattery = null;

        // Recalculate column sums
        controller.OnBatteryPlaced();

        Debug.Log($"Battery removed from Row {row}, Col {column}");
    }

    /// <summary>Clears this socket and sends the battery back to the tray.</summary>
    public void ClearSocket(Transform tray)
    {
        if (currentBattery != null)
        {
            BatteryDragDrop drag = currentBattery.GetComponent<BatteryDragDrop>();
            if (drag != null) drag.Unlock(tray);
            currentBattery = null;
        }
    }
}
