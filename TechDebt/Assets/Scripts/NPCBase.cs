// NPCBase.cs
using UnityEngine;
using System.Collections.Generic;

public abstract class NPCBase : MonoBehaviour
{
    public event System.Action OnDestinationReached;

    public float movementSpeed = 3f;
    public bool isMoving { get; private set; } = false;

    private List<Vector3> currentPath;
    private int pathIndex;

    protected virtual void Update()
    {
        HandleMovement();
    }
    
    public void MoveTo(Vector3 destination)
    {
        List<Vector3> path = Pathfinding.FindPath(transform.position, destination);
        Debug.Log("Moving To " + destination + " with path " + path.Count);
        if (path != null && path.Count > 0)
        {
            currentPath = path;
            pathIndex = 0;
            isMoving = true;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} could not find a path to {destination}.");
            isMoving = false;
        }
    }

    public void StopMovement()
    {
        isMoving = false;
        currentPath = null;
        pathIndex = 0;
    }

    private void HandleMovement()
    {
        if (!isMoving || currentPath == null || pathIndex >= currentPath.Count)
        {
            return;
        }

        Vector3 targetWaypoint = currentPath[pathIndex];
        targetWaypoint.z = transform.position.z; // Maintain original Z to prevent visual glitches

        if (Vector3.Distance(transform.position, targetWaypoint) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetWaypoint, movementSpeed * Time.deltaTime);
        }
        else
        {
            pathIndex++;
            if (pathIndex >= currentPath.Count)
            {
                isMoving = false;
                currentPath = null;
                OnDestinationReached?.Invoke();
            }
        }
    }
}