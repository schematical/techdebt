using UnityEngine;
using UnityEngine.InputSystem;

    public class CameraController : MonoBehaviour
{
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float zoomSpeed = 0.5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 12f;
    
    private Camera mainCamera;
    private Vector3 lastPanPosition;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("CameraController: Main Camera component not found!");
            this.enabled = false;
        }
    }
    void Update()
    {
        HandlePan();
        HandleZoom();
        HandleKeyboardInput();
        HandleKeyboardZoom();
    }

    void HandleKeyboardInput()
    {
        Vector3 movement = Vector3.zero;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) movement += Vector3.up;
        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) movement += Vector3.down;
        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) movement += Vector3.left;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) movement += Vector3.right;
        
        transform.position += movement.normalized * panSpeed * Time.deltaTime;
    }

    void HandleKeyboardZoom()
    {
        float zoomDelta = 0;
        if (Keyboard.current.qKey.isPressed) zoomDelta = 1;
        else if (Keyboard.current.eKey.isPressed) zoomDelta = -1;

        if (zoomDelta != 0)
        {
            float newSize = mainCamera.orthographicSize - zoomDelta * zoomSpeed * Time.deltaTime * 10;
            mainCamera.orthographicSize = Mathf.Clamp(newSize, minZoom, maxZoom);
        }
    }

    void HandlePan()
    {
        if (Mouse.current == null) return;
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        
        if (mousePosition.x < 1 || mousePosition.x > mainCamera.pixelWidth - 1 || 
            mousePosition.y < 1 || mousePosition.y > mainCamera.pixelHeight - 1) return;

        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            lastPanPosition = mainCamera.ScreenToWorldPoint(mousePosition);
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
