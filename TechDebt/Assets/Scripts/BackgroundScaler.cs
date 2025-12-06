// BackgroundScaler.cs
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("BackgroundScaler: SpriteRenderer or sprite is missing!");
            return;
        }

        // Get the camera's orthographic size and aspect ratio
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("BackgroundScaler: Main Camera not found! Make sure your camera is tagged 'MainCamera'.");
            return;
        }
        
        float cameraHeight = mainCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Get the sprite's original size
        float spriteHeight = sr.sprite.bounds.size.y;
        float spriteWidth = sr.sprite.bounds.size.x;

        // Calculate the scale needed to fit the camera's view
        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        // Apply the larger of the two scales to ensure the background covers the entire screen
        float scale = Mathf.Max(scaleX, scaleY);
        transform.localScale = new Vector3(scale, scale, 1);
        
        Debug.Log($"Background scaled to {scale} to fit camera view.");
    }
}
