using UnityEngine;
using UnityEngine.InputSystem; // Using the new Input System

public class CameraController : MonoBehaviour
{
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float zoomSpeed = 0.5f; // Adjusted for new input system's delta values
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 12f;

    // --- Update-based Animation State ---
    private bool _isZooming = false;
    private Transform _zoomTarget;
    private float _zoomDuration;
    private float _zoomElapsedTime;
    private Vector3 _startPosition;
    private float _startZoom;
    private float _targetZoom;
    private bool _followAfterZoom;
    // ------------------------------------

    private Vector3 lastPanPosition;
    private Vector2 initialRightClickMousePosition; // Store initial mouse screen position
    private Camera mainCamera;
    private Transform targetToFollow;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("CameraController: Main Camera not found!");
            this.enabled = false;
        }
    }
    
    public void ZoomTo(Transform target)
    {
        StopFollowing(); // Stop any current following
        
        _isZooming = true;
        _zoomTarget = target;
        _zoomDuration = 2f;
        _zoomElapsedTime = 0f;
        _followAfterZoom = false; // Ensure this is reset
        
        _startPosition = transform.position;
        _startZoom = mainCamera.orthographicSize;
        _targetZoom = 4f; // Sensible default close-up zoom
    }

    public void ZoomToAndFollow(Transform target)
    {
        ZoomTo(target);
        _followAfterZoom = true;
    }

    void Update()
    {
        // Pressing Escape also stops following
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            StopFollowing();
            _isZooming = false; // Also cancel zoom animation
        }

        if (_isZooming)
        {
            HandleZoomToAnimation();
        }
        else // Only handle manual controls if not animating
        {
            HandlePan();
            HandleZoom();
            HandleKeyboardInput();
            HandleKeyboardZoom();
        }
    }

    private void HandleZoomToAnimation()
    {
        _zoomElapsedTime += Time.deltaTime;
        float t = _zoomElapsedTime / _zoomDuration;

        // SmoothStep interpolation for a nice ease-in and ease-out effect
        t = t * t * (3f - 2f * t);

        // Interpolate position
        Vector3 targetPos = new Vector3(_zoomTarget.position.x, _zoomTarget.position.y, _startPosition.z);
        transform.position = Vector3.Lerp(_startPosition, targetPos, t);

        // Interpolate zoom
        mainCamera.orthographicSize = Mathf.Lerp(_startZoom, _targetZoom, t);

        // Check if animation is complete
        if (_zoomElapsedTime >= _zoomDuration)
        {
            _isZooming = false;
            // Ensure final position and zoom are set precisely
            transform.position = new Vector3(_zoomTarget.position.x, _zoomTarget.position.y, _startPosition.z);
            mainCamera.orthographicSize = _targetZoom;

            if (_followAfterZoom)
            {
                StartFollowing(_zoomTarget);
                _followAfterZoom = false; // Reset the flag
            }
        }
    }
    
    // Using LateUpdate for following to ensure the target has finished its movement for the frame.
    // This prevents camera jitter.
    void LateUpdate()
    {
        // Do not interfere if a zoom animation is in progress
        if (_isZooming) return;
        
        if (targetToFollow != null)
        {
            // Keep the camera's original Z position
            transform.position = new Vector3(targetToFollow.position.x, targetToFollow.position.y, transform.position.z);
        }
    }

    public void StartFollowing(Transform target)
    {

        targetToFollow = target;
    }

    public void StopFollowing()
    {

        targetToFollow = null;
        _isZooming = false;
    }

    void HandleKeyboardInput()
    {
        Vector3 movement = Vector3.zero;

        // WASD input
        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
        {
            movement += Vector3.up;
        }
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
        {
            movement += Vector3.down;
        }
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
        {
            movement += Vector3.left;
        }
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
        {
            movement += Vector3.right;
        }

        if (movement.sqrMagnitude > 0.01f)
        {
            // Apply movement scaled by panSpeed and Time.deltaTime
            GameManager.Instance.cameraController.StopFollowing();
            transform.position += movement.normalized * panSpeed * Time.deltaTime;
        }
    }

    void HandleKeyboardZoom()
    {
        float zoomDelta = 0;
        if (Keyboard.current.qKey.isPressed)
        {
            zoomDelta = 1; // Zoom in
        }
        else if (Keyboard.current.eKey.isPressed)
        {
            zoomDelta = -1; // Zoom out
        }

        if (zoomDelta != 0)
        {
            float newSize = mainCamera.orthographicSize - zoomDelta * zoomSpeed * Time.deltaTime * 10; // Multiply by 10 to make it faster
            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    void HandlePan()
    {
        if (Mouse.current == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        if (mousePosition.x < 1 || mousePosition.x > mainCamera.pixelWidth - 1 || 
            mousePosition.y < 1 || mousePosition.y > mainCamera.pixelHeight - 1)
        {
            return;
        }

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            GameManager.Instance.cameraController.StopFollowing();
            lastPanPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            initialRightClickMousePosition = mousePosition; // Store initial screen position
        }

        if (Mouse.current.rightButton.isPressed)
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