// ParticleSystemFactory.cs
using UnityEngine;

public class ParticleSystemFactory : MonoBehaviour
{
    /// <summary>
    /// Creates a particle system configured for displaying text from a render texture.
    /// </summary>
    /// <param name="parent">The parent transform for the new particle system GameObject.</param>
    /// <param name="renderMaterial">The material that uses the render texture.</param>
    /// <returns>A configured ParticleSystem.</returns>
    public static ParticleSystem Create(GameObject parent, Material renderMaterial)
    {
        var go = new GameObject("TextParticleSystem");
        go.transform.SetParent(parent.transform);

        var ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 2.0f; // Increased lifetime
        main.startSpeed = 0.1f; // Decreased initial speed
        main.startSize = 5f; // Increased size
        main.maxParticles = 10;
        
        var emission = ps.emission;
        emission.enabled = false; // We will emit manually

        // Make particles move upwards
        var force = ps.forceOverLifetime;
        force.enabled = true;
        force.y = new ParticleSystem.MinMaxCurve(0.8f); // Decreased upward force

        // Assign the special material that uses the Render Texture
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = renderMaterial;
        
        return ps;
    }
}
