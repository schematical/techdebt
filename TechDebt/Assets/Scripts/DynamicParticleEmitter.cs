// DynamicParticleEmitter.cs
using UnityEngine;
using TMPro;

public class DynamicParticleEmitter : MonoBehaviour
{
    public static DynamicParticleEmitter Instance { get; private set; }

    [Header("Scene References")]
    public Camera particleUICamera;
    public TextMeshProUGUI particleText;
    public ParticleSystem textParticleSystem;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Optional: if you want it to persist across scenes
        }

        if (particleUICamera == null || particleText == null || textParticleSystem == null)
        {
            Debug.LogError("DynamicParticleEmitter is missing essential references (Camera, Text, or ParticleSystem). Please assign them in the inspector.");
        }
    }

    /// <summary>
    /// Emits a particle with the specified text at a given world position.
    /// </summary>
    /// <param name="text">The text to display on the particle.</param>
    /// <param name="position">The world position to emit the particle from.</param>
    public void Emit(string text, Vector3 position)
    {
        if (textParticleSystem == null || particleText == null) return;

        // 1. Update the UI Text
        particleText.text = text;

        // 2. Position the Particle System emitter
        textParticleSystem.transform.position = position;

        // 3. Emit one particle
        // The particle will capture the current state of the render texture.
        textParticleSystem.Emit(1);
    }
}
