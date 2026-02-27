using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class IslandPortal : MonoBehaviour
{
    [Header("Scene to load")]
    public string sceneName = "Forest_Minimap";

    [Header("Tap settings")]
    public float maxTapMovePixels = 15f;   // prevents triggering when scrolling
    public float maxTapTime = 0.35f;

    private Vector2 startPos;
    private float startTime;
    private bool pressedOnThis;

    void Update()
    {
        // Touch
        var touch = Touchscreen.current;
        if (touch != null)
        {
            var t = touch.primaryTouch;

            if (t.press.wasPressedThisFrame)
            {
                startPos = t.position.ReadValue();
                startTime = Time.time;
                pressedOnThis = HitThis(startPos);
            }

            if (t.press.wasReleasedThisFrame && pressedOnThis)
            {
                TryActivate(t.position.ReadValue());
            }

            return;
        }

        // Mouse
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.wasPressedThisFrame)
        {
            startPos = mouse.position.ReadValue();
            startTime = Time.time;
            pressedOnThis = HitThis(startPos);
        }

        if (mouse.leftButton.wasReleasedThisFrame && pressedOnThis)
        {
            TryActivate(mouse.position.ReadValue());
        }
    }

    private void TryActivate(Vector2 endPos)
    {
        float move = Vector2.Distance(startPos, endPos);
        float time = Time.time - startTime;

        // If user was dragging to scroll, do nothing
        if (move > maxTapMovePixels || time > maxTapTime) return;

        SceneManager.LoadScene(sceneName);
    }

    private bool HitThis(Vector2 screenPos)
    {
        Vector3 world = Camera.main.ScreenToWorldPoint(screenPos);
        RaycastHit2D hit = Physics2D.Raycast(world, Vector2.zero);
        return hit.collider != null && hit.collider.gameObject == gameObject;
    }
}
