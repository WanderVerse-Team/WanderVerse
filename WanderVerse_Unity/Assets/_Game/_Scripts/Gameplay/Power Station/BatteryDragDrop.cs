using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.localScale = Vector3.one;
        if (originalSizeDelta != Vector2.zero)
            rectTransform.sizeDelta = originalSizeDelta;

        Vector3 beginDragLocalPos = rectTransform.localPosition;
        rectTransform.localPosition = new Vector3(beginDragLocalPos.x, beginDragLocalPos.y, 0f);

        MoveToPointer(eventData);

        // Render on top of everything
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveToPointer(eventData);
    }

    private void MoveToPointer(PointerEventData eventData)
    {
        if (canvasRectTransform == null || rectTransform == null || eventData == null) return;

        Camera eventCamera = GetEventCamera(eventData);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                eventData.position,
                eventCamera,
                out Vector2 localPoint))
        {
            rectTransform.anchoredPosition = localPoint;
            Vector3 dragLocalPos = rectTransform.localPosition;
            rectTransform.localPosition = new Vector3(dragLocalPos.x, dragLocalPos.y, 0f);
        }
    }

    private Camera GetEventCamera(PointerEventData eventData)
    {
        if (eventData != null)
        {
            if (eventData.pressEventCamera != null)
                return eventData.pressEventCamera;

            if (eventData.enterEventCamera != null)
                return eventData.enterEventCamera;
        }

        if (parentCanvas != null)
        {
            if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;

            if (parentCanvas.worldCamera != null)
                return parentCanvas.worldCamera;
        }

        return Camera.main;
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

        EnsureSocketsExistInScene();

        BatterySocket[] sockets = FindObjectsByType<BatterySocket>(FindObjectsSortMode.None);
        if (sockets == null || sockets.Length == 0)
        {
            Debug.LogWarning("[BatteryDragDrop] No BatterySocket components found at drop time.");
            return false;
        }

        Vector2 releaseScreenPoint = eventData != null ? eventData.position : Vector2.zero;

        // 0) Fast path: socket directly under pointerEnter hierarchy.
        if (eventData != null && eventData.pointerEnter != null)
        {
            BatterySocket pointerSocket = eventData.pointerEnter.GetComponentInParent<BatterySocket>();
            if (pointerSocket != null && pointerSocket.gameObject.activeInHierarchy && pointerSocket.currentBattery == null)
            {
                if (pointerSocket.TryAcceptBattery(this, batteryIdentity))
                    return true;
            }
        }

        // 1) Preferred: use EventSystem raycast results at release point.
        if (EventSystem.current != null)
        {
            PointerEventData releaseEvent = new PointerEventData(EventSystem.current)
            {
                position = releaseScreenPoint
            };

            List<RaycastResult> hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(releaseEvent, hits);

            for (int i = 0; i < hits.Count; i++)
            {
                GameObject hitObject = hits[i].gameObject;
                if (hitObject == null) continue;

                BatterySocket hitSocket = hitObject.GetComponentInParent<BatterySocket>();
                if (hitSocket == null) continue;
                if (!hitSocket.gameObject.activeInHierarchy) continue;
                if (hitSocket.currentBattery != null) continue;

                if (hitSocket.TryAcceptBattery(this, batteryIdentity))
                    return true;
            }
        }

        BatterySocket nearestSocket = null;
        float nearestDistance = float.MaxValue;
        BatterySocket bestExpandedRectSocket = null;
        float expandedRectNearestDistance = float.MaxValue;

        foreach (BatterySocket socket in sockets)
        {
            if (socket == null || !socket.gameObject.activeInHierarchy) continue;
            if (socket.currentBattery != null) continue;

            RectTransform socketRect = socket.transform as RectTransform;
            if (socketRect == null) continue;

            bool containsReleasePoint = RectTransformUtility.RectangleContainsScreenPoint(
                socketRect,
                releaseScreenPoint,
                eventData != null ? eventData.enterEventCamera : null);

            bool inExpandedRect = IsWithinExpandedRect(socketRect, releaseScreenPoint, eventData != null ? eventData.enterEventCamera : null, 80f);

            if (containsReleasePoint)
            {
                if (socket.TryAcceptBattery(this, batteryIdentity))
                    return true;
            }

            if (inExpandedRect)
            {
                float screenDistance = Vector2.Distance(releaseScreenPoint, RectTransformUtility.WorldToScreenPoint(eventData != null ? eventData.enterEventCamera : null, socketRect.position));
                if (screenDistance < expandedRectNearestDistance)
                {
                    expandedRectNearestDistance = screenDistance;
                    bestExpandedRectSocket = socket;
                }
            }

            float distance = Vector2.Distance(rectTransform.position, socketRect.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestSocket = socket;
            }
        }

        if (bestExpandedRectSocket != null && bestExpandedRectSocket.TryAcceptBattery(this, batteryIdentity))
            return true;

        // 2) Fallback: snap to nearest empty socket with dynamic threshold.
        float maxSnapDistance = 240f;
        if (nearestSocket != null)
        {
            RectTransform nearestRect = nearestSocket.transform as RectTransform;
            if (nearestRect != null)
            {
                Vector2 rectSize = nearestRect.rect.size;
                float sizeBasedThreshold = Mathf.Max(rectSize.x, rectSize.y) * 1.25f;
                if (sizeBasedThreshold > maxSnapDistance)
                    maxSnapDistance = sizeBasedThreshold;
            }
        }

        if (nearestSocket != null && nearestDistance <= maxSnapDistance)
        {
            if (nearestSocket.TryAcceptBattery(this, batteryIdentity))
                return true;
        }

        return false;
    }

    private bool IsWithinExpandedRect(RectTransform rect, Vector2 screenPoint, Camera eventCamera, float padding)
    {
        if (rect == null) return false;

        Vector3[] corners = new Vector3[4];
        rect.GetWorldCorners(corners);

        Vector2 min = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]);
        Vector2 max = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[2]);

        Rect expanded = Rect.MinMaxRect(
            Mathf.Min(min.x, max.x) - padding,
            Mathf.Min(min.y, max.y) - padding,
            Mathf.Max(min.x, max.x) + padding,
            Mathf.Max(min.y, max.y) + padding);

        return expanded.Contains(screenPoint);
    }

    private void EnsureSocketsExistInScene()
    {
        RectTransform[] rects = FindObjectsByType<RectTransform>(FindObjectsSortMode.None);
        foreach (RectTransform rect in rects)
        {
            if (rect == null) continue;
            if (!TryParseSocketName(rect.name, out int row, out int column)) continue;

            BatterySocket socket = rect.GetComponent<BatterySocket>();
            if (socket == null)
                socket = rect.gameObject.AddComponent<BatterySocket>();

            socket.row = row;
            socket.column = column;
        }
    }

    private bool TryParseSocketName(string objectName, out int row, out int column)
    {
        row = -1;
        column = -1;

        if (string.IsNullOrEmpty(objectName)) return false;
        if (!objectName.StartsWith("Socket_R", System.StringComparison.OrdinalIgnoreCase)) return false;

        string[] parts = objectName.Split('_');
        if (parts.Length < 3) return false;

        string rowPart = parts[1];
        string colPart = parts[2];

        if (rowPart.Length < 2 || colPart.Length < 2) return false;
        if (!int.TryParse(rowPart.Substring(1), out row)) return false;
        if (!int.TryParse(colPart.Substring(1), out column)) return false;

        return true;
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
