using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Drop zone for deleting batteries. Supports dragging batteries out of sockets into a dustbin.
/// </summary>
public class DustbinDropZone : MonoBehaviour, IDropHandler
{
    [Tooltip("Optional explicit reference. Auto-found if left empty.")]
    public PowerStationController controller;

    [Tooltip("If true, tray is refilled back to capacity after discarding.")]
    public bool refillTrayAfterDiscard = true;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        BatteryDragDrop drag = dropped.GetComponent<BatteryDragDrop>();
        BatteryIdentity id = dropped.GetComponent<BatteryIdentity>();
        if (drag == null || id == null) return;

        if (controller == null)
            controller = FindAnyObjectByType<PowerStationController>();

        if (drag.CurrentSocket != null)
            drag.CurrentSocket.currentBattery = null;

        Destroy(dropped);

        if (controller != null)
            controller.OnBatteryDiscarded(refillTrayAfterDiscard);
    }
}
