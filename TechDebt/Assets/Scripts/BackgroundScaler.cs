// BackgroundScaler.cs
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BackgroundScaler : MonoBehaviour
{
    private SpriteRenderer sr;
    private Camera mainCamera;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        if (sr == null || sr.sprite == null)
        {
            Debug.LogError("BackgroundScaler: SpriteRenderer or sprite is missing!");
            enabled = false; // Disable the script if components are missing
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError("BackgroundScaler: Main Camera not found! Make sure your camera is tagged 'MainCamera'.");
            enabled = false; // Disable the script if camera is missing
            return;
        }
    }

    void Update()
    {
        // Get camera dimensions
        float cameraHeight = mainCamera.orthographicSize * 2;
        float cameraWidth = cameraHeight * mainCamera.aspect;

        // Get sprite dimensions
        float spriteHeight = sr.sprite.bounds.size.y;
        float spriteWidth = sr.sprite.bounds.size.x;

        // Calculate the scale needed for each dimension independently
        float scaleX = cameraWidth / spriteWidth;
        float scaleY = cameraHeight / spriteHeight;

        // Apply the independent scales to stretch the image to fill the screen
        transform.localScale = new Vector3(scaleX, scaleY, 1);
    }
}
