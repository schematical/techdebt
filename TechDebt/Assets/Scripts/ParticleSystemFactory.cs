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
        main.startLifetime = 1.2f;
        main.startSpeed = 1.5f;
        main.startSize = 0.7f;
        main.maxParticles = 500;
        
        var emission = ps.emission;
        emission.enabled = false; // We will emit manually

        // Make particles move upwards
        var force = ps.forceOverLifetime;
        force.enabled = true;
        force.y = new ParticleSystem.MinMaxCurve(1.5f);

        // Assign the special material that uses the Render Texture
        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.material = renderMaterial;
        
        return ps;
    }
}
