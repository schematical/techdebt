// CameraController.cs
using UnityEngine;
using UnityEngine.InputSystem; // Using the new Input System

public class CameraController : MonoBehaviour
{
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float zoomSpeed = 0.5f; // Adjusted for new input system's delta values
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 12f;

    private Vector3 lastPanPosition;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("CameraController: Main Camera not found!");
            this.enabled = false;
        }
    }

    void Update()
    {
        HandlePan();
        HandleZoom();
    }

    void HandlePan()
    {
        if (Mouse.current == null) return;

        // --- NEW: Guard Clause to prevent 'out of view frustum' warning ---
        // Get the current mouse position
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        // Check if the mouse is within the screen's rectangle.
        // We add a small border (1 pixel) to avoid issues at the very edge.
        if (mousePosition.x < 1 || mousePosition.x > mainCamera.pixelWidth - 1 || 
            mousePosition.y < 1 || mousePosition.y > mainCamera.pixelHeight - 1)
        {
            return; // Mouse is outside the game window, so don't process panning.
        }
        // --- End of new code ---

        if (Mouse.current.middleButton.wasPressedThisFrame)
        {
            lastPanPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        }

        if (Mouse.current.middleButton.isPressed)
        {
            Vector3 newPanPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            Vector3 panDelta = lastPanPosition - newPanPosition;
            
            transform.position += panDelta;
        }
    }

    void HandleZoom()
    {
        if (Mouse.current == null) return;

        float scrollValue = Mouse.current.scroll.ReadValue().y;
        
        if (scrollValue != 0)
        {
            float scrollDelta = Mathf.Sign(scrollValue);
            float newSize = mainCamera.orthographicSize - scrollDelta * zoomSpeed;
            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }
}
