// Server.cs
using UnityEngine;

public class Desk : InfrastructureInstance
{
    public void OnResearchProgress(Vector3 position)
    {
        // Emit a "+1" text particle at the given position.
        DynamicParticleEmitter.Instance.Emit("+1", position);
    }
}