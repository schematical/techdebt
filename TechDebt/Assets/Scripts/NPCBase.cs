// NPCBase.cs
using UnityEngine;
using System.Collections.Generic;

public abstract class NPCBase : MonoBehaviour
{
    public enum State
    {
        Idle,
        ExecutingTask,
        Wandering,
        Exiting,
        Exited
    }
    public State CurrentState { get; set; } = State.Idle;
    public event System.Action OnDestinationReached;

    public float movementSpeed = 3f;
    public bool isMoving { get; private set; } = false;

    private List<Vector3> currentPath;
    private int pathIndex;
    


    protected virtual void Update()
    {
        HandleMovement();
    }
    public void Wander()
    {

        Vector3 wanderDestination = GetRandomWalkablePoint(transform.position, 10f);
  
        if (!Vector3.zero.Equals(wanderDestination))
        {
            CurrentState = State.Wandering;
            MoveTo(wanderDestination);
        }
    }
    
    public void NavigateToDoor()
    {
        InfrastructureInstance doorInstance = GameManager.Instance.GetInfrastructureInstanceByID("door");
        if (doorInstance != null)
        {
            // The base NPCTask constructor and OnStart will handle movement to the destination
            MoveTo(doorInstance.transform.position);
         
            Debug.Log($"{gameObject.name} is navigating to the door.");
        }
        else
        {
            Debug.LogError($"NavigateToDoorTask: Door infrastructure with ID 'door' not found.");
           
        }
        
    }


    private Vector3 GetRandomWalkablePoint(Vector3 origin, float radius)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += origin;

            Node node = GridManager.Instance.NodeFromWorldPoint(randomDirection);
            if (node != null && node.isWalkable)
            {
                return randomDirection;
            }
        }
        
        return Vector3.zero;
    }

    public virtual void OnPlayPhaseStart()
    {
 
        StopMovement();
        CurrentState = State.Idle;
    }
    public void MoveTo(Vector3 destination)
    {
        List<Vector3> path = Pathfinding.FindPath(transform.position, destination);
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