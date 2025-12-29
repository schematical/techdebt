// DeskOverlayController.cs
using UnityEngine;

public class DeskOverlayController : MonoBehaviour
{
    private Camera _mainCamera;

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

    // This method will be called to position the overlay relative to the camera view
    public void PositionOverlayForCamera(Vector3 cameraTargetPosition, float cameraTargetSize)
    {
        if (_mainCamera == null) return;

        // Position it at the camera's Z position or slightly closer
        transform.position = new Vector3(cameraTargetPosition.x, cameraTargetPosition.y, _mainCamera.transform.position.z + 1);
        
        // --- DEBUG LOGS START ---
        Debug.Log("--- Debugging Overlay Scale ---");
        Debug.Log($"Camera Orthographic Size (cameraTargetSize): {cameraTargetSize}");
        Debug.Log($"Camera Aspect Ratio: {_mainCamera.aspect}");

        // Scale the overlay to fit the camera's orthographic size
        float screenHeightInWorldUnits = cameraTargetSize * 2;
        float screenWidthInWorldUnits = screenHeightInWorldUnits * _mainCamera.aspect;
        Debug.Log($"Calculated Screen World Size: Width={screenWidthInWorldUnits}, Height={screenHeightInWorldUnits}");

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float spriteWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
            float spriteHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
            Debug.Log($"Sprite World Size: Width={spriteWidth}, Height={spriteHeight}");

            if (spriteWidth > 0 && spriteHeight > 0)
            {
                float scaleX = screenWidthInWorldUnits / spriteWidth;
                float scaleY = screenHeightInWorldUnits / spriteHeight;
                Debug.Log($"Final Calculated Scale: X={scaleX}, Y={scaleY}");

                transform.localScale = new Vector3(scaleX, scaleY, 1);
            }
        }
        else
        {
            Debug.LogError("SpriteRenderer or Sprite is missing on the overlay prefab!");
        }
        Debug.Log("--- End Debugging ---");
    }
}

