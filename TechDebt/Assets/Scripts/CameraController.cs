using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public enum CameraState { Game, Desk, TransitioningToDesk, TransitioningToGame }
    
    [SerializeField] private float panSpeed = 20f;
    [SerializeField] private float zoomSpeed = 0.5f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 12f;

    [SerializeField] private float targetDeskZoom = 2.5f;
    [SerializeField] private float transitionDuration = 0.5f;

    private Camera mainCamera;
    private DeskOverlayController _deskOverlayController;
    private Vector3 lastPanPosition;
    
    // State Management
    private CameraState _currentState = CameraState.Game;
    private float _transitionTimer;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _startZoom;
    private float _targetZoom;
    
    // Stored Game View
    private Vector3 _lastGameViewPosition;
    private float _lastGameViewZoom;

    void Start()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("CameraController: Main Camera not found!");
            this.enabled = false;
            return;
        }
        _deskOverlayController = GameManager.Instance.DeskOverlayController;
        if (_deskOverlayController == null)
        {
            Debug.LogError("CameraController: DeskOverlayController not found in scene!");
        }
        
        _lastGameViewPosition = transform.position;
        _lastGameViewZoom = mainCamera.orthographicSize;
    }

    void Update()
    {
        if (_currentState == CameraState.TransitioningToDesk || _currentState == CameraState.TransitioningToGame)
        {
            HandleTransition();
        }
        else if (_currentState == CameraState.Game)
        {
            HandlePan();
            HandleZoom();
            HandleKeyboardInput();
            HandleKeyboardZoom();
        }
    }

    private void HandleTransition()
    {
        _transitionTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_transitionTimer / transitionDuration);

        transform.position = Vector3.Lerp(_startPosition, _targetPosition, t);
        mainCamera.orthographicSize = Mathf.Lerp(_startZoom, _targetZoom, t);
        _deskOverlayController?.PositionOverlayForCamera(transform.position, mainCamera.orthographicSize);

        if (t >= 1f)
        {
            _currentState = (_currentState == CameraState.TransitioningToDesk) ? CameraState.Desk : CameraState.Game;
        }
    }

    public void TransitionToDeskView()
    {
        if (_currentState == CameraState.TransitioningToDesk || _currentState == CameraState.Desk) return;

        _lastGameViewPosition = transform.position;
        _lastGameViewZoom = mainCamera.orthographicSize;

        _currentState = CameraState.TransitioningToDesk;
        _transitionTimer = 0f;
        _startPosition = transform.position;
        _startZoom = mainCamera.orthographicSize;
        _targetPosition = new Vector3(0, 0, transform.position.z);
        _targetZoom = targetDeskZoom;
    }

    public void TransitionToGameView()
    {
        if (_currentState == CameraState.TransitioningToGame || _currentState == CameraState.Game) return;

        _currentState = CameraState.TransitioningToGame;
        _transitionTimer = 0f;
        _startPosition = transform.position;
        _startZoom = mainCamera.orthographicSize;
        _targetPosition = _lastGameViewPosition;
        _targetZoom = _lastGameViewZoom;
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
