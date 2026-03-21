using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Handles drag-and-drop for battery UI elements.
/// Uses the same IPointer pattern as DragDropItem in Hungry Golem.
/// Works with Unity UI (Canvas-based).
/// </summary>
public class BatteryDragDrop : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Canvas parentCanvas;
    private RectTransform canvasRectTransform;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private BatteryIdentity batteryIdentity;
    private Vector2 originalSizeDelta;
    private Vector2 dragPointerOffset;
    private Vector3 originalPosition;
    private Transform originalParent;
    private bool isPlaced = false;
    private BatterySocket currentSocket;

    public BatterySocket CurrentSocket => currentSocket;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup   = GetComponent<CanvasGroup>();
        batteryIdentity = GetComponent<BatteryIdentity>();
        parentCanvas  = GetComponentInParent<Canvas>();
        canvasRectTransform = parentCanvas != null ? parentCanvas.GetComponent<RectTransform>() : null;
        originalSizeDelta = rectTransform != null ? rectTransform.sizeDelta : Vector2.zero;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (rectTransform == null) return;

        if (parentCanvas == null)
            parentCanvas = GetComponentInParent<Canvas>();
        if (canvasRectTransform == null && parentCanvas != null)
            canvasRectTransform = parentCanvas.GetComponent<RectTransform>();

        originalPosition = rectTransform.position;
        originalParent   = transform.parent;

        // If this battery is currently in a socket, clear that socket occupancy while dragging.
        if (currentSocket == null && originalParent != null)
            currentSocket = originalParent.GetComponent<BatterySocket>();

        if (currentSocket != null)
        {
            currentSocket.currentBattery = null;

            PowerStationController controller = FindAnyObjectByType<PowerStationController>();
            if (controller != null)
                controller.OnBatteryPlaced();

            isPlaced = false;
        }

        // Let raycasts pass through so sockets can detect the drop
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;

        if (parentCanvas != null)
            transform.SetParent(parentCanvas.transform, true);

        dragPointerOffset = (Vector2)rectTransform.position - eventData.position;

        // Render on top of everything
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (canvasRectTransform == null || rectTransform == null) return;

        Vector2 screenPosition = eventData.position + dragPointerOffset;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPosition,
                eventData.pressEventCamera,
                out Vector2 localPoint))
        {
            rectTransform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        // Fallback: direct snap check in case socket OnDrop doesn't fire due UI raycast setup
        if (TrySnapToSocket(eventData))
            return;

        // If not snapped into a socket, return to the tray
        if (!isPlaced)
        {
            BatterySocket originalSocket = originalParent != null ? originalParent.GetComponent<BatterySocket>() : null;
            if (originalSocket != null && batteryIdentity != null)
            {
                if (originalSocket.TryAcceptBattery(this, batteryIdentity))
                    return;
            }

            transform.SetParent(originalParent, true);
            rectTransform.position = originalPosition;
        }
    }

    private bool TrySnapToSocket(PointerEventData eventData)
    {
        if (batteryIdentity == null) return false;

        BatterySocket[] sockets = FindObjectsByType<BatterySocket>(FindObjectsSortMode.None);
        foreach (BatterySocket socket in sockets)
        {
            if (socket == null || !socket.gameObject.activeInHierarchy) continue;

            RectTransform socketRect = socket.transform as RectTransform;
            if (socketRect == null) continue;

            if (!RectTransformUtility.RectangleContainsScreenPoint(socketRect, eventData.position, eventData.enterEventCamera))
                continue;

            if (socket.TryAcceptBattery(this, batteryIdentity))
                return true;
        }

        return false;
    }

    /// <summary>Called by BatterySocket when this battery is accepted into a slot.</summary>
    public void LockIntoSocket(Transform socketTransform)
    {
        isPlaced = true;
        currentSocket = socketTransform != null ? socketTransform.GetComponent<BatterySocket>() : null;

        // Keep under canvas so socket parent scale does not squash battery size.
        if (parentCanvas != null)
            transform.SetParent(parentCanvas.transform, true);

        if (socketTransform != null)
            rectTransform.position = socketTransform.position;

        rectTransform.localScale = Vector3.one;
        if (originalSizeDelta != Vector2.zero)
            rectTransform.sizeDelta = originalSizeDelta;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }

    /// <summary>Called to release the battery back to the tray (on reset).</summary>
    public void Unlock(Transform trayParent)
    {
        isPlaced = false;
        currentSocket = null;
        transform.SetParent(trayParent, false);
        rectTransform.localScale = Vector3.one;
        if (originalSizeDelta != Vector2.zero)
            rectTransform.sizeDelta = originalSizeDelta;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }
}
