// DeskOverlayController.cs
using UnityEngine;

public class DeskOverlayController : MonoBehaviour
{
    private Camera _mainCamera;
    public Transform screenChildSurface; // Assign the screen child GameObject here in the Inspector of the prefab

    void Awake()
    {
        _mainCamera = Camera.main;
        if (_mainCamera == null)
        {
            Debug.LogError("DeskOverlayController: Main Camera not found!");
            return;
        }

        // Set up rendering order
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingLayerName = "Overlay"; 
            sr.sortingOrder = 100;
        }
        
        gameObject.SetActive(false); // Start hidden
    }

    public void ShowOverlay()
    {
        gameObject.SetActive(true);
    }

    public void HideOverlay()
    {
        gameObject.SetActive(false);
    }

    public void SetupScreen(RenderTexture renderTexture, DeskCameraController deskCameraControllerInstance)
    {
        if (screenChildSurface == null)
        {
            Debug.LogError("DeskOverlayController: 'Screen Child Surface' is not assigned! Cannot setup screen.");
            return;
        }

        Renderer screenRenderer = screenChildSurface.GetComponent<Renderer>();
        if (screenRenderer != null)
        {
            Material screenMaterial = new Material(Shader.Find("Unlit/Texture"));
            screenMaterial.mainTexture = renderTexture;
            screenRenderer.material = screenMaterial;
            deskCameraControllerInstance.SetMonitorTransform(screenChildSurface.transform);
        }
        else
        {
            Debug.LogError("DeskOverlayController: 'Screen Child Surface' has no Renderer component. Cannot setup screen.");
        }
    }

    // This method will be called to position the overlay relative to the camera view
    public void PositionOverlayForCamera(Vector3 cameraTargetPosition, float cameraTargetSize)
    {
        if (_mainCamera == null) return;

        // Position it at the camera's Z position or slightly closer
        transform.position = new Vector3(cameraTargetPosition.x, cameraTargetPosition.y, _mainCamera.transform.position.z + 1);
        
        // Scale the overlay to fit the camera's orthographic size
        float screenHeightInWorldUnits = cameraTargetSize * 2;
        float screenWidthInWorldUnits = screenHeightInWorldUnits * _mainCamera.aspect;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float spriteWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
            float spriteHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;

            if (spriteWidth > 0 && spriteHeight > 0)
            {
                float scaleX = screenWidthInWorldUnits / spriteWidth;
                float scaleY = screenHeightInWorldUnits / spriteHeight;

                transform.localScale = new Vector3(scaleX, scaleY, 1);
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer or Sprite is missing on the overlay prefab!");
        }
    }
}

