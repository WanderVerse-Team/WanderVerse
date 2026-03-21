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
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 originalPosition;
    private Transform originalParent;
    private bool isPlaced = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup   = GetComponent<CanvasGroup>();
        parentCanvas  = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        originalPosition = rectTransform.position;
        originalParent   = transform.parent;

        // Let raycasts pass through so sockets can detect the drop
        canvasGroup.blocksRaycasts = false;

        // Render on top of everything
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isPlaced) return;
        rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isPlaced) return;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        // If not snapped into a socket, return to the tray
        if (!isPlaced)
        {
            transform.SetParent(originalParent, true);
            rectTransform.position = originalPosition;
        }
    }

    /// <summary>Called by BatterySocket when this battery is accepted into a slot.</summary>
    public void LockIntoSocket(Transform socketTransform)
    {
        isPlaced = true;
        transform.SetParent(socketTransform, false);
        rectTransform.anchoredPosition = Vector2.zero; // Center inside socket
        rectTransform.localScale = Vector3.one;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = false;         // Prevent re-dragging
    }

    /// <summary>Called to release the battery back to the tray (on reset).</summary>
    public void Unlock(Transform trayParent)
    {
        isPlaced = false;
        transform.SetParent(trayParent, false);
        rectTransform.localScale = Vector3.one;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
    }
}
