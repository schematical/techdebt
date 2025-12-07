// NPCBase.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class NPCBase : MonoBehaviour
{
    public float movementSpeed = 3f;
    private Coroutine movementCoroutine;
    protected bool isMoving = false;

    public void MoveTo(Vector3 destination)
    {
        List<Vector3> path = Pathfinding.FindPath(transform.position, destination);
        if (path != null && path.Count > 0)
        {
            if (movementCoroutine != null)
            {
                StopCoroutine(movementCoroutine);
            }
            movementCoroutine = StartCoroutine(FollowPath(path));
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} could not find a path to {destination}.");
        }
    }

    private IEnumerator FollowPath(List<Vector3> path)
    {
        isMoving = true;
        int targetIndex = 0;
        while (targetIndex < path.Count)
        {
            Vector3 currentWaypoint = path[targetIndex];
            // Adjust Z-axis to stay at 0, preventing sorting issues.
            currentWaypoint.z = 0;

            while (Vector3.Distance(transform.position, currentWaypoint) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, movementSpeed * Time.deltaTime);
                yield return null; // Wait for the next frame
            }
            targetIndex++;
        }
        Debug.Log($"{gameObject.name} reached its destination.");
        isMoving = false;
    }
}