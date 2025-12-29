// DeskCameraController.cs
using UnityEngine;

public class DeskCameraController : MonoBehaviour
{
    public enum CameraState { Desk, Game, TransitioningToDesk, TransitioningToGame }

    [SerializeField] private float transitionDuration = 0.5f;
    private Transform monitorTransform;
    [SerializeField] private float deskZoom = 5f;
    [SerializeField] private float gameZoom = 1.5f;

    private Camera _deskCamera;
    private CameraState _currentState = CameraState.Desk;
    private float _transitionTimer;
    private Vector3 _startPosition;
    private Vector3 _targetPosition;
    private float _startZoom;
    private float _targetZoom;

    public void SetMonitorTransform(Transform newMonitorTransform)
    {
        monitorTransform = newMonitorTransform;
    }

    void Start()
    {
        _deskCamera = GetComponent<Camera>();
        if (_deskCamera == null)
        {
            Debug.LogError("DeskCameraController: Camera component not found!");
            this.enabled = false;
        }
    }

    void Update()
    {
        if (_currentState == CameraState.TransitioningToDesk || _currentState == CameraState.TransitioningToGame)
        {
            HandleTransition();
        }
    }

    private void HandleTransition()
    {
        _transitionTimer += Time.deltaTime;
        float t = Mathf.Clamp01(_transitionTimer / transitionDuration);

        transform.position = Vector3.Lerp(_startPosition, _targetPosition, t);
        _deskCamera.orthographicSize = Mathf.Lerp(_startZoom, _targetZoom, t);

        if (t >= 1f)
        {
            transform.position = _targetPosition;
            _deskCamera.orthographicSize = _targetZoom;
            _currentState = (_currentState == CameraState.TransitioningToDesk) ? CameraState.Desk : CameraState.Game;
        }
    }

    public void TransitionToDeskView()
    {
        if (_currentState == CameraState.TransitioningToDesk || _currentState == CameraState.Desk) return;

        _currentState = CameraState.TransitioningToDesk;
        _transitionTimer = 0f;

        _startPosition = transform.position;
        _startZoom = _deskCamera.orthographicSize;

        _targetPosition = new Vector3(0, 0, transform.position.z);
        _targetZoom = deskZoom;
    }

    public void TransitionToGameView()
    {
        if (_currentState == CameraState.TransitioningToGame || _currentState == CameraState.Game) return;

        _currentState = CameraState.TransitioningToGame;
        _transitionTimer = 0f;
        
        _startPosition = transform.position;
        _startZoom = _deskCamera.orthographicSize;

        _targetPosition = new Vector3(monitorTransform.position.x, monitorTransform.position.y, transform.position.z);
        _targetZoom = gameZoom;
    }
}
