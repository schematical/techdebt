// NPCBase.cs
using UnityEngine;

// This will be the base class for all Non-Player Characters in the game.
// We can add shared logic like movement, pathfinding, and health here later.
public abstract class NPCBase : MonoBehaviour
{
    // Example property that all NPCs might share
    public float movementSpeed = 5f;

    public virtual void MoveTo(Vector3 destination)
    {
        // Pathfinding logic will go here in the future.
        Debug.Log($"{gameObject.name} is moving to {destination}");
    }
}
